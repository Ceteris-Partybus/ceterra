using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BoardContext : NetworkedSingleton<BoardContext> {
    protected override bool ShouldPersistAcrossScenes => true;

    public enum State {
        PLAYER_TURN,
        PLAYER_MOVING,
        PAUSED
    }

    private State currentState = State.PLAYER_TURN;
    public State CurrentState => currentState;

    private FieldBehaviourList fieldBehaviourList;
    public FieldBehaviourList FieldBehaviourList {
        get => fieldBehaviourList;
        set => fieldBehaviourList ??= value;
    }

    #region Global Stats
    public const int MAX_STATS_VALUE = 100;

    [Header("Global Stats")]
    [SerializeField]
    [SyncVar(hook = nameof(OnFundsStatChanged))]
    private int fundsStat;
    public int FundsStat => fundsStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnResourceStatChanged))]
    private int resourceStat;
    public int ResourceStat => resourceStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnEconomyStatChanged))]
    private int economyStat;
    public int EconomyStat => economyStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnSocietyStatChanged))]
    private int societyStat;
    public int SocietyStat => societyStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnEnvironmentStatChanged))]
    private int environmentStat;
    public int EnvironmentStat => environmentStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnResourceNextRoundChanged))]
    private int resourcesNextRound;
    public int ResourcesNextRound => resourcesNextRound;

    #endregion

    #region Global Stats Hooks

    private void OnFundsStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateFundsValue(new_);
    }
    private void OnResourceStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateResourceValue(new_);
    }
    private void OnEconomyStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateTrend("trend-economy", old, new_);
        BoardOverlay.Instance.UpdateEconomyValue(new_);
    }
    private void OnSocietyStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateTrend("trend-society", old, new_);
        BoardOverlay.Instance.UpdateSocietyValue(new_);
    }
    private void OnEnvironmentStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateTrend("trend-environment", old, new_);
        BoardOverlay.Instance.UpdateEnvironmentValue(new_);
    }
    private void OnResourceNextRoundChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateResourcesNextRoundValue();
    }

    #endregion

    [Header("Current Player")]
    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    [SerializeField]
    private int currentPlayerId = 0;
    public int CurrentPlayerId => currentPlayerId;

    [Header("Movement Tracking")]
    [SerializeField]
    private int totalMovementsCompleted = 0;

    public readonly SyncList<Investment> investments = new SyncList<Investment>();
    public readonly SyncList<Event> events = new SyncList<Event>();
    public readonly SyncList<FundsHistoryEntry> fundsHistory = new SyncList<FundsHistoryEntry>();
    public readonly SyncList<ResourceHistoryEntry> resourceHistory = new SyncList<ResourceHistoryEntry>();

    public override void OnStartServer() {
        investments.OnInsert += OnInvestmentItemInserted;
        fundsHistory.OnAdd += OnFundsHistoryItemAdded;
        resourceHistory.OnAdd += OnResourceHistoryItemAdded;

        investments.AddRange(Investment.LoadInvestmentsFromResources());
        Debug.Log($"Loaded {investments.Count} investments from resources");
        events.AddRange(Event.LoadEventsFromResources());
        Debug.Log($"Loaded {events.Count} events from resources");
    }

    private void OnInvestmentItemInserted(int index) {
        int investmentId = investments[index].id;
        RpcRecalculateInvestment(investmentId);
    }

    private void OnFundsHistoryItemAdded(int index) {
        RpcUpdateFundsHistory(fundsHistory[index]);
    }

    private void OnResourceHistoryItemAdded(int index) {
        RpcUpdateResourceHistory(resourceHistory[index]);
    }

    [ClientRpc]
    public void RpcRecalculateInvestment(int investmentId) {
        BoardOverlay.Instance.RecalculateInvestment(investmentId);
    }

    [ClientRpc]
    public void RpcUpdateFundsHistory(FundsHistoryEntry entry) {
        BoardOverlay.Instance.UpdateFundsHistory(entry);
    }

    [ClientRpc]
    public void RpcUpdateResourceHistory(ResourceHistoryEntry entry) {
        BoardOverlay.Instance.UpdateResourceHistory(entry);
    }

    protected override void Start() {
        base.Start();
        currentPlayerId = GameManager.Singleton.PlayerIds[0];

        this.resourcesNextRound = 50;
        this.fundsStat = 0;
        this.resourceStat = 0;
        this.economyStat = 50;
        this.societyStat = 50;
        this.environmentStat = 50;
    }

    [Server]
    public void StartPlayerTurn() {
        currentState = State.PLAYER_TURN;
        RpcNotifyPlayerTurn(currentPlayerId);
    }

    [Server]
    public void ProcessDiceRoll(BoardPlayer player, int diceValue) {
        if (currentState != State.PLAYER_TURN) {
            return;
        }

        if (currentPlayerId != player.PlayerId) {
            return;
        }

        currentState = State.PLAYER_MOVING;
        player.MoveToField(diceValue);
    }

    [Server]
    public void NextPlayerTurn() {
        int[] playerIds = GameManager.Singleton.PlayerIds;
        int indexInLobby = System.Array.IndexOf(playerIds, currentPlayerId);
        currentPlayerId = playerIds[(indexInLobby + 1) % playerIds.Length];

        StartPlayerTurn();
    }

    [Server]
    public void OnPlayerMovementComplete(BoardPlayer player) {
        if (currentState == State.PLAYER_MOVING && currentPlayerId == player.PlayerId) {
            totalMovementsCompleted++;

            // Check if all players have had one movement
            int totalPlayers = GameManager.Singleton.PlayerIds.Length;
            if (totalMovementsCompleted >= totalPlayers) {
                totalMovementsCompleted = 0;

                UpdateResourceStat((int)resourcesNextRound);
                ResourceHistoryEntry entry = new ResourceHistoryEntry(resourcesNextRound, HistoryEntryType.DEPOSIT, "Rundenende");
                this.resourceHistory.Add(entry);
                resourcesNextRound = CalculateResourcesNextRound();

                int completedInvestments = 0;
                foreach (var investment in investments) {
                    investment.Tick();
                    if (investment.cooldown == 0 && !investment.completed) {
                        ApplyInvestment(investment);
                        investment.completed = true;
                        RpcShowInvestInfo($"Das Investment {investment.displayName} wurde fertiggestellt!");
                        completedInvestments++;
                    }
                    TriggerInvestmentListUpdate(investments.IndexOf(investment), investment);
                }

                if (completedInvestments > 0) {
                    StartCoroutine(WaitBeforeMinigame(completedInvestments * 3f));
                }
                else {
                    GameManager.Singleton.StartMinigame("MgQuizduel");
                }
                return;
            }

            NextPlayerTurn();
        }
    }

    [Server]
    private IEnumerator WaitBeforeMinigame(float seconds) {
        yield return new WaitForSeconds(seconds);
        GameManager.Singleton.StartMinigame("MgQuizduel");
    }

    [Server]
    private int CalculateResourcesNextRound() {
        int resourcesToAdd = 25 + (int)(225 * Mathf.Pow(economyStat / 100f, 2));
        return resourcesToAdd;
    }

    [ClientRpc]
    public void RpcNotifyPlayerTurn(int playerId) {
    }

    public void OnCurrentPlayerChanged(int _, int newPlayerId) {
        BoardPlayer newPlayer = GetPlayerById(newPlayerId);
        if (newPlayer == null) {
            Debug.LogWarning($"No player found with ID {newPlayerId}");
            return;

        }
        CurrentTurnManager.Instance.UpdateCurrentPlayerName(newPlayer.PlayerName);
        CurrentTurnManager.Instance.AllowRollDiceButtonFor(newPlayerId);
    }

    public void OnStateChanged(State oldState, State newState) {
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (currentState != State.PLAYER_TURN) { return false; }

        return player.PlayerId == currentPlayerId;
    }

    public BoardPlayer GetCurrentPlayer() {
        return GetPlayerById(currentPlayerId);
    }

    public BoardPlayer GetLocalPlayer() {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public BoardPlayer GetPlayerById(int playerId) {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault(p => p.PlayerId == playerId);
    }

    #region Global Stat Update

    public void UpdateFundsStat(int amount) {
        fundsStat = Mathf.Max(0, fundsStat + amount);
    }

    public void UpdateResourceStat(int amount) {
        resourceStat = Mathf.Max(0, resourceStat + amount);
    }

    public void UpdateEconomyStat(int amount) {
        economyStat = Mathf.Clamp(economyStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateSocietyStat(int amount) {
        societyStat = Mathf.Clamp(societyStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateEnvironmentStat(int amount) {
        environmentStat = Mathf.Clamp(environmentStat + amount, 0, MAX_STATS_VALUE);
    }

    [ServerCallback]
    internal void ShowQuizForPlayer(int playerId) {
        var player = GetPlayerById(playerId);
        RpcShowQuizForPlayer(player.connectionToClient, player);
    }

    [TargetRpc]
    private void RpcShowQuizForPlayer(NetworkConnectionToClient target, BoardPlayer player) {
        Debug.Log("Calling InitializeQuizForPlayer");
        Debug.Log($"BoardQuizController is {BoardquizController.Instance}");
        BoardquizController.Instance.InitializeQuizForPlayer(player);
        Debug.Log("Calling DoStartQuiz");
        BoardquizController.Instance.DoStartQuiz();
    }

    #endregion

    #region Investment Management

    [Server]
    public void InvestInInvestment(BoardPlayer player, int investmentId, int amount) {
        int index = -1;
        Investment investment = null;

        for (int i = 0; i < investments.Count; i++) {
            if (investments[i].id == investmentId) {
                investment = investments[i];
                index = i;
                break;
            }
        }

        if (investment == null) {
            throw new Exception($"No investment found with ID {investmentId}");
        }

        int surplus = investment.Invest(amount);
        int coinsToRemove = amount - Math.Max(surplus, 0);
        player.RemoveCoins(coinsToRemove);

        TriggerInvestmentListUpdate(index, investment);
    }

    [Server]
    private void TriggerInvestmentListUpdate(int index, Investment investment) {
        investments.RemoveAt(index);
        investments.Insert(index, investment);
    }

    [Server]
    public void ApproveInvestment(int investmentId, int coins) {
        Investment investment = investments.FirstOrDefault(inv => inv.id == investmentId);
        int index = investments.IndexOf(investment);

        UpdateFundsStat(-coins);
        FundsHistoryEntry fundsEntry = new FundsHistoryEntry(coins, HistoryEntryType.WITHDRAW, $"Resourcen für {investment.displayName}");
        this.fundsHistory.Add(fundsEntry);

        investment.Invest(coins);

        if (investment.fullyFinanced) {
            UpdateResourceStat(-investment.requiredResources);
            ResourceHistoryEntry entry = new ResourceHistoryEntry(investment.requiredResources, HistoryEntryType.WITHDRAW, $"Finanzierung {investment.displayName}");
            this.resourceHistory.Add(entry);
            investment.inConstruction = true;
            RpcShowInvestInfo("Das Investment wurde vollständig finanziert und befindet sich nun im Bau.");
        }
        else {
            RpcShowInvestInfo($"Das Investment wurde mit {coins} Münzen finanziert, benötigt aber noch weitere Gelder. Die Ressourcen wurden noch nicht abgezogen.");
        }
        TriggerInvestmentListUpdate(index, investment);
    }

    [ClientRpc]
    private void RpcShowInvestInfo(string message) {
        if (!string.IsNullOrEmpty(message)) {
            InfoModal.Instance.Message = message;
            ModalManager.Instance.Show(InfoModal.Instance);
        }
    }

    [Server]
    private void ApplyInvestment(Investment investment) {
        foreach (InvestmentModifier modifier in investment.modifier) {
            switch (modifier.Type) {
                case InvestmentType.ECONOMY:
                    UpdateEconomyStat(modifier.Magnitude);
                    break;
                case InvestmentType.SOCIETY:
                    UpdateSocietyStat(modifier.Magnitude);
                    break;
                case InvestmentType.ENVIRONMENT:
                    UpdateEnvironmentStat(modifier.Magnitude);
                    break;
            }
        }
    }

    #endregion

    #region Event Management

    [Server]
    public void TriggerRandomEvent() {
        var possibleEvents = events.Where(e => e.canOccur).ToList();
        if (possibleEvents.Count == 0) {
            Debug.LogWarning("No possible events to trigger.");
            return;
        }

        int totalWeight = possibleEvents.Sum(e => e.weight);
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var eventOption in possibleEvents) {
            cumulativeWeight += eventOption.weight;
            if (randomValue < cumulativeWeight) {
                Debug.Log($"Triggering event: {eventOption.title}");
                TriggerEvent(eventOption.id);
                eventOption.MarkOccurrence();
                break;
            }
        }
    }

    [Server]
    private void TriggerEvent(int eventId) {
        Event eventToTrigger = events.FirstOrDefault(e => e.id == eventId);
        if (eventToTrigger == null) {
            throw new Exception($"No event found with ID {eventId}");
        }

        foreach (EventModifier modifier in eventToTrigger.modifier) {

            int multiplier = modifier.Effect == EventEffect.INCREASES ? 1 : -1;
            int calculatedValue = modifier.Magnitude * multiplier;

            switch (modifier.Type) {
                case EventType.FUNDS:
                    UpdateFundsStat(calculatedValue);
                    break;
                case EventType.RESOURCE:
                    UpdateResourceStat(calculatedValue);
                    break;
                case EventType.ECONOMY:
                    UpdateEconomyStat(calculatedValue);
                    break;
                case EventType.SOCIETY:
                    UpdateSocietyStat(calculatedValue);
                    break;
                case EventType.ENVIRONMENT:
                    UpdateEnvironmentStat(calculatedValue);
                    break;
            }
        }

        RpcShowEventInfo(eventToTrigger);
    }

    [ClientRpc]
    private void RpcShowEventInfo(Event eventToShow) {
        EventModal.Instance.Title = eventToShow.title;
        EventModal.Instance.Description = eventToShow.description;
        ModalManager.Instance.Show(EventModal.Instance);
    }

    #endregion
}