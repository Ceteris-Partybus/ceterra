using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class BoardContext : NetworkedSingleton<BoardContext> {
    protected override bool ShouldPersistAcrossScenes => true;

    public enum State {
        PLAYER_TURN,
        PLAYER_MOVING,
        PAUSED,
        MINIGAME,
        MINIGAME_FINISHED
    }

    private State currentState = State.PLAYER_TURN;
    public State CurrentState {
        get => currentState;
        set { currentState = value; }
    }

    private FieldBehaviourList fieldBehaviourList;
    public FieldBehaviourList FieldBehaviourList {
        get => fieldBehaviourList;
        set => fieldBehaviourList ??= value;
    }

    #region Global Stats
    public const int MAX_STATS_VALUE = 100;

    private readonly SyncList<CyberneticEffect> cyberneticEffects = new();
    private bool isApplyingCyberneticEffects = false;

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

    [SerializeField]
    [SyncVar]
    private Trend economyTrend;
    public Trend EconomyTrend => economyTrend;

    [SerializeField]
    [SyncVar]
    private Trend societyTrend;
    public Trend SocietyTrend => societyTrend;

    [SerializeField]
    [SyncVar]
    private Trend environmentTrend;
    public Trend EnvironmentTrend => environmentTrend;

    #endregion

    #region Global Stats Hooks

    private void OnFundsStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateFundsValue(new_);
    }
    private void OnResourceStatChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateResourceValue(new_);
    }
    private void OnEconomyStatChanged(int old, int new_) {
        economyTrend = CalculateTrend(old, new_);
        BoardOverlay.Instance.UpdateEconomyValue(new_);
        BoardOverlay.Instance.UpdateTrends();
        CmdUpdateEconomyTrend(economyTrend);
    }
    private void OnSocietyStatChanged(int old, int new_) {
        societyTrend = CalculateTrend(old, new_);
        BoardOverlay.Instance.UpdateSocietyValue(new_);
        BoardOverlay.Instance.UpdateTrends();
        CmdUpdateSocietyTrend(societyTrend);
    }
    private void OnEnvironmentStatChanged(int old, int new_) {
        environmentTrend = CalculateTrend(old, new_);
        BoardOverlay.Instance.UpdateEnvironmentValue(new_);
        BoardOverlay.Instance.UpdateTrends();
        CmdUpdateEnvironmentTrend(environmentTrend);
    }
    private void OnResourceNextRoundChanged(int old, int new_) {
        BoardOverlay.Instance.UpdateResourcesNextRoundValue();
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateEconomyTrend(Trend trend) {
        economyTrend = trend;
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateSocietyTrend(Trend trend) {
        societyTrend = trend;
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateEnvironmentTrend(Trend trend) {
        environmentTrend = trend;
    }

    private Trend CalculateTrend(int oldValue, int newValue) {
        if (newValue > oldValue) {
            return Trend.RISING;
        }
        else if (newValue < oldValue) {
            return Trend.FALLING;
        }
        return Trend.NEUTRAL;
    }

    #endregion

    [Header("Current Player")]
    [SyncVar]
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

        economyTrend = Trend.NEUTRAL;
        societyTrend = Trend.NEUTRAL;
        environmentTrend = Trend.NEUTRAL;

        investments.AddRange(Investment.LoadInvestmentsFromResources());
        Debug.Log($"Loaded {investments.Count} investments from resources");
        events.AddRange(Event.LoadEventsFromResources());
        Debug.Log($"Loaded {events.Count} events from resources");

        List<CyberneticEffect> effects = new() {
            // Economy -> Resources: Strong economy boosts resources (+5 to +15 per round when economy 50-100)
            new CyberneticEffect {
                source = CyberneticEffectType.ECONOMY,
                target = CyberneticEffectType.RESOURCE,
                effectCurve = AnimationCurve.Linear(0, 0, 1, 1),
                effectMultiplier = 15f,
                description = "Eine starke Wirtschaft steigert die Ressourcenproduktion.",
                requirement = new CyberneticRequirement(50, 100)
            },
            // Economy -> Environment: Strong economy damages environment (-3 to -8 per round when economy 50-100)
            new CyberneticEffect {
                source = CyberneticEffectType.ECONOMY,
                target = CyberneticEffectType.ENVIRONMENT,
                effectCurve = AnimationCurve.Linear(0, 0, 1, 1),
                effectMultiplier = -8f,
                description = "Eine starke Wirtschaft kann die Umwelt belasten.",
                requirement = new CyberneticRequirement(50, 100)
            },
            // Low Society -> Economy: Weak society hurts economy (-3 to -10 per round when society 0-49)
            new CyberneticEffect {
                source = CyberneticEffectType.SOCIETY,
                target = CyberneticEffectType.ECONOMY,
                effectCurve = AnimationCurve.Linear(0, 1, 1, 0),
                effectMultiplier = -10f,
                description = "Eine schwache Gesellschaft schadet der Wirtschaft.",
                requirement = new CyberneticRequirement(0, 49)
            },
            // High Society -> Economy: Strong society boosts economy (+2 to +5 per round when society 50-100)
            new CyberneticEffect {
                source = CyberneticEffectType.SOCIETY,
                target = CyberneticEffectType.ECONOMY,
                effectCurve = AnimationCurve.Linear(0, 0, 1, 1),
                effectMultiplier = 5f,
                description = "Eine starke Gesellschaft fördert das Wirtschaftswachstum.",
                requirement = new CyberneticRequirement(50, 100)
            },
            // Environment -> Society: Good environment improves society (+2 to +6 per round when environment 50-100)
            new CyberneticEffect {
                source = CyberneticEffectType.ENVIRONMENT,
                target = CyberneticEffectType.SOCIETY,
                effectCurve = AnimationCurve.Linear(0, 0, 1, 1),
                effectMultiplier = 6f,
                description = "Eine gesunde Umwelt verbessert das Wohlbefinden der Gesellschaft.",
                requirement = new CyberneticRequirement(50, 100)
            },
            // Society -> Environment: Strong society promotes environmental protection (+2 to +5 per round when society 50-100)
            new CyberneticEffect {
                source = CyberneticEffectType.SOCIETY,
                target = CyberneticEffectType.ENVIRONMENT,
                effectCurve = AnimationCurve.Linear(0, 0, 1, 1),
                effectMultiplier = 5f,
                description = "Eine starke Gesellschaft fördert den Umweltschutz.",
                requirement = new CyberneticRequirement(50, 100)
            }
        };

        cyberneticEffects.AddRange(effects);
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

    [Server]
    private void ApplyCyberneticEffects() {
        if (isApplyingCyberneticEffects) {
            return;
        }

        isApplyingCyberneticEffects = true;

        int economyChange = 0;
        int societyChange = 0;
        int environmentChange = 0;
        int resourceChange = 0;

        foreach (var effect in cyberneticEffects) {
            Debug.Log($"Applying cybernetic effect: {effect.description}");
            float sourceValue = GetStatValue(effect.source);

            if (sourceValue < effect.requirement.minValue || sourceValue > effect.requirement.maxValue) {
                Debug.Log($"Skipping effect {effect.description} due to unmet requirement: {sourceValue} not in [{effect.requirement.minValue}, {effect.requirement.maxValue}]");
                continue;
            }

            // Normalize the source value within the requirement range to 0-1
            float normalizedValue = (sourceValue - effect.requirement.minValue) /
                                   (effect.requirement.maxValue - effect.requirement.minValue);

            // Evaluate the curve with the normalized value (0-1)
            float effectStrength = effect.effectCurve.Evaluate(normalizedValue);
            Debug.Log($"Effect strength based on {effect.source} value {sourceValue} (normalized: {normalizedValue}): {effectStrength}");

            // Apply the multiplier to get the final change value
            int change = Mathf.RoundToInt(effectStrength * effect.effectMultiplier);
            Debug.Log($"Calculated change for {effect.target}: {change}");

            switch (effect.target) {
                case CyberneticEffectType.ECONOMY:
                    economyChange += change;
                    break;
                case CyberneticEffectType.SOCIETY:
                    societyChange += change;
                    break;
                case CyberneticEffectType.ENVIRONMENT:
                    environmentChange += change;
                    break;
                case CyberneticEffectType.RESOURCE:
                    resourceChange += change;
                    break;
            }
        }

        Debug.Log($"Total Cybernetic Effects - Economy: {economyChange}, Society: {societyChange}, Environment: {environmentChange}, Resource: {resourceChange}");

        if (economyChange != 0) {
            UpdateEconomyStat(economyChange);
        }
        if (societyChange != 0) {
            UpdateSocietyStat(societyChange);
        }
        if (environmentChange != 0) {
            UpdateEnvironmentStat(environmentChange);
        }
        if (resourceChange != 0) {
            UpdateResourceStat(resourceChange);
        }

        isApplyingCyberneticEffects = false;
    }

    private float GetStatValue(CyberneticEffectType type) {
        return type switch {
            CyberneticEffectType.ECONOMY => economyStat,
            CyberneticEffectType.SOCIETY => societyStat,
            CyberneticEffectType.ENVIRONMENT => environmentStat,
            CyberneticEffectType.RESOURCE => resourceStat,
            _ => 0f,
        };
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
        StartCoroutine(DelayedRpcNotify());

        IEnumerator DelayedRpcNotify() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.roomSlots.Count);
            RpcNotifyPlayerTurn(currentPlayerId, GameManager.Singleton.CurrentRound, GameManager.Singleton.MaxRounds);
        }
    }

    [Server]
    public void ProcessDiceRoll(BoardPlayer player, int diceValue) {
        if (currentState != State.PLAYER_TURN || currentPlayerId != player.PlayerId) {
            return;
        }

        currentState = State.PLAYER_MOVING;
        player.PlayerMovement.MoveToField(diceValue);
    }

    [Server]
    private void DetermineNextPlayer() {
        var playerIds = GameManager.Singleton.PlayerIds;
        var indexInLobby = Array.IndexOf(playerIds, currentPlayerId);
        currentPlayerId = playerIds[(indexInLobby + 1) % playerIds.Length];
    }

    [Server]
    public void OnPlayerMovementComplete(BoardPlayer player) {
        if (currentState == State.PLAYER_MOVING && currentPlayerId == player.PlayerId) {
            totalMovementsCompleted++;
            DetermineNextPlayer();

            var totalPlayers = GameManager.Singleton.PlayerIds.Length;
            if (totalMovementsCompleted >= totalPlayers) {
                totalMovementsCompleted = 0;

                StartCoroutine(OnRoundCompleted());
                return;
            }
            StartPlayerTurn();
        }
    }

    [Server]
    public IEnumerator OnRoundCompleted() {
        yield return StartCoroutine(ProcessInvestments());
        ApplyCyberneticEffects();

        UpdateResourceStat(resourcesNextRound);

        string resourceHistoryEntryName = LocalizationManager.Instance.GetLocalizedText(56652191783813120);
        resourceHistory.Add(new ResourceHistoryEntry(resourcesNextRound, HistoryEntryType.DEPOSIT, resourceHistoryEntryName));
        resourcesNextRound = CalculateResourcesNextRound();

        yield return StartCoroutine(StartMinigame());
        GameManager.Singleton.IncrementRound();
        if (GameManager.Singleton.CurrentRound > GameManager.Singleton.MaxRounds) {
            GameManager.Singleton.StopGameSwitchEndScene();
            yield break;
        }
    }

    [Server]
    private IEnumerator ProcessInvestments() {
        var completedInvestments = new List<Investment>();
        foreach (var investment in investments.Where(inv => !inv.completed)) {
            investment.Tick();
            if (investment.cooldown == 0) {
                ApplyInvestment(investment);
                investment.completed = true;
                completedInvestments.Add(investment);
            }
            TriggerInvestmentListUpdate(investments.IndexOf(investment), investment);
        }

        if (completedInvestments.Count == 0) { yield break; }

        var investementInfo = completedInvestments.Select(investment => LocalizationManager.Instance.GetLocalizedText(56652337988861952, new object[] { investment.displayName })).Aggregate((a, b) => a + "\n" + b);
        RpcShowInvestInfo(investementInfo);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
    }

    [Server]
    private IEnumerator StartMinigame() {
        currentState = State.MINIGAME;
        GameManager.Singleton.StartMinigame();
        yield return new WaitUntil(() => currentState == State.MINIGAME_FINISHED);
    }

    [Server]
    private int CalculateResourcesNextRound() {
        int resourcesToAdd = 25 + (int)(225 * Mathf.Pow(economyStat / 100f, 2));
        return resourcesToAdd;
    }

    public Action<BoardPlayer, int, int> OnNextPlayerTurn;
    [ClientRpc]
    public void RpcNotifyPlayerTurn(int playerId, int currentRound, int maxRounds) {
        StartCoroutine(InvokeAfterInitialization());

        IEnumerator InvokeAfterInitialization() {
            yield return new WaitUntil(() => CurrentTurnManager.Instance != null && CurrentTurnManager.Instance.IsInitialized);
            OnNextPlayerTurn?.Invoke(GetPlayerById(playerId), currentRound, maxRounds);
        }
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        return currentState == State.PLAYER_TURN && player.PlayerId == currentPlayerId;
    }

    public BoardPlayer GetCurrentPlayer() {
        return GetPlayerById(currentPlayerId);
    }

    public BoardPlayer GetLocalPlayer() {

        IEnumerator WaitForAnyPlayer() {
            yield return new WaitUntil(() => FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0);
        }

        StartCoroutine(WaitForAnyPlayer());

        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public List<BoardPlayer> GetRemotePlayers() {
        return GetAllPlayers().Where(p => !p.isLocalPlayer).ToList();
    }

    public List<BoardPlayer> GetAllPlayers() {

        IEnumerator WaitForPlayers() {
            yield return new WaitUntil(() => FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length == GameManager.Singleton.roomSlots.Count());
        }

        StartCoroutine(WaitForPlayers());

        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
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
        player.PlayerStats.ModifyCoins(-coinsToRemove);
        player.PlayerStats.ModifyScore(coinsToRemove / 10);

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
        string fundsHistoryEntryName = LocalizationManager.Instance.GetLocalizedText(56659020039421952, new object[] { investment.displayName });
        FundsHistoryEntry fundsEntry = new FundsHistoryEntry(coins, HistoryEntryType.WITHDRAW, fundsHistoryEntryName);
        this.fundsHistory.Add(fundsEntry);

        investment.Invest(coins);

        if (investment.fullyFinanced) {
            UpdateResourceStat(-investment.requiredResources);
            string resourceHistoryEntryName = LocalizationManager.Instance.GetLocalizedText(56659352559648768, new object[] { investment.displayName });
            ResourceHistoryEntry entry = new ResourceHistoryEntry(investment.requiredResources, HistoryEntryType.WITHDRAW, resourceHistoryEntryName);
            this.resourceHistory.Add(entry);
            investment.inConstruction = true;
            RpcShowInvestInfo(LocalizationManager.Instance.GetLocalizedText(56661867099422720));
        }
        else {
            RpcShowInvestInfo(LocalizationManager.Instance.GetLocalizedText(56661867099422721, new object[] { coins }));
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

    public bool IsAnyPlayerMoving() {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Any(p => p.IsMoving);
    }

    public bool IsAnyPlayerInAnimation() {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Any(p => p.IsAnimationFinished == false);
    }

    public bool IsAnyPlayerChoosingJunction() {
        return FindObjectsByType<JunctionFieldBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Any(j => j.IsWaitingForBranchChoice);
    }

    #region Event Management

    [Server]
    public void TriggerRandomEvent() {
        var possibleEvents = events.Where(e => e.canOccur).ToList();

        if (possibleEvents.Count == 0) {
            Debug.Log("All events have occurred the maximum number of times. Resetting occurrences.");
            foreach (var ev in events) {
                ev.ResetOccurrences();
            }
            possibleEvents = events.ToList();
        }

        int totalWeight = possibleEvents.Sum(e => e.weight);
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var eventOption in possibleEvents) {
            cumulativeWeight += eventOption.weight;
            if (randomValue < cumulativeWeight) {
                Debug.Log($"Triggering event: {LocalizationManager.Instance.GetLocalizedText(eventOption.title)}");
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
        EventModal.Instance.Title = LocalizationManager.Instance.GetLocalizedText(eventToShow.title);
        EventModal.Instance.Description = LocalizationManager.Instance.GetLocalizedText(eventToShow.description);
        ModalManager.Instance.Show(EventModal.Instance);
    }

    #endregion

    public int EvaluateGlobalScore() {
        float weightedScore = (environmentStat * 0.5f) + (economyStat * 0.3f) + (societyStat * 0.2f);

        if (economyStat >= 60 && societyStat >= 60 && environmentStat >= 60) {
            weightedScore *= 1.15f;
        }

        if (environmentStat < 30) {
            weightedScore *= 0.7f;
        }

        return Mathf.RoundToInt(Mathf.Clamp(weightedScore, 0, MAX_STATS_VALUE));
    }
}