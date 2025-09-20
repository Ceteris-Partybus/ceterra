using Mirror;
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
    public const uint MAX_STATS_VALUE = 100;

    [Header("Global Stats")]
    [SerializeField]
    [SyncVar(hook = nameof(OnFundsStatChanged))]
    private uint fundsStat;
    public uint FundsStat => fundsStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnResourceStatChanged))]
    private uint resourceStat;
    public uint ResourceStat => resourceStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnEconomyStatChanged))]
    private uint economyStat;
    public uint EconomyStat => economyStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnSocietyStatChanged))]
    private uint societyStat;
    public uint SocietyStat => societyStat;

    [SerializeField]
    [SyncVar(hook = nameof(OnEnvironmentStatChanged))]
    private uint environmentStat;
    public uint EnvironmentStat => environmentStat;

    #endregion

    #region Global Stats Hooks

    private void OnFundsStatChanged(uint old, uint new_) {
        BoardOverlay.Instance.UpdateFundsValue(new_);
    }
    private void OnResourceStatChanged(uint old, uint new_) {
        BoardOverlay.Instance.UpdateResourceValue(new_);
    }
    private void OnEconomyStatChanged(uint old, uint new_) {
        BoardOverlay.Instance.UpdateEconomyValue(new_);
    }
    private void OnSocietyStatChanged(uint old, uint new_) {
        BoardOverlay.Instance.UpdateSocietyValue(new_);
    }
    private void OnEnvironmentStatChanged(uint old, uint new_) {
        BoardOverlay.Instance.UpdateEnvironmentValue(new_);
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

    protected override void Start() {
        base.Start();
        
        // Initialize stats first
        this.fundsStat = 0;
        this.resourceStat = 0;
        this.economyStat = 50;
        this.societyStat = 50;
        this.environmentStat = 50;

        // Wait a frame to ensure all players are initialized
        StartCoroutine(InitializeCurrentPlayer());

        // TODO: Remove, this is just for testing purposes
        if (isServer) {
            StartCoroutine(StartValuesIncrease());
        }
    }

    [Server]
    private IEnumerator InitializeCurrentPlayer() {
        // Wait a few frames to ensure all players are spawned and initialized
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Try to find any available player and set as current
        var allPlayers = FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allPlayers.Length > 0) {
            currentPlayerId = allPlayers[0].PlayerId;
            Debug.Log($"Initialized current player ID to {currentPlayerId}");
            StartPlayerTurn();
        } else {
            Debug.LogWarning("No BoardPlayers found to initialize current player");
        }
    }

    protected void OnDestroy() {
        // Stop all coroutines to prevent issues during cleanup
        StopAllCoroutines();
    }

    // Override OnDisable to ensure proper cleanup
    protected void OnDisable() {
        // Stop coroutines when disabled
        StopAllCoroutines();
    }

    [ServerCallback]
    private IEnumerator StartValuesIncrease() {
        while (true) {
            yield return new WaitForSeconds(1f);
            UpdateFundsStat(1);
            UpdateResourceStat(1);
            UpdateEconomyStat(1);
            UpdateSocietyStat(1);
            UpdateEnvironmentStat(1);
        }
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
        if (GameManager.Singleton?.PlayerIds == null || GameManager.Singleton.PlayerIds.Length == 0) {
            Debug.LogWarning("Cannot advance to next player: PlayerIds is null or empty");
            return;
        }
        
        int[] playerIds = GameManager.Singleton.PlayerIds;
        int indexInLobby = System.Array.IndexOf(playerIds, currentPlayerId);
        currentPlayerId = playerIds[(indexInLobby + 1) % playerIds.Length];

        StartPlayerTurn();
    }

    [Server]
    public void OnPlayerMovementComplete(BoardPlayer player) {
        if (currentState == State.PLAYER_MOVING &&
            currentPlayerId == player.PlayerId) {

            totalMovementsCompleted++;

            // Check if all players have had one movement
            if (GameManager.Singleton?.PlayerIds != null && GameManager.Singleton.PlayerIds.Length > 0) {
                int totalPlayers = GameManager.Singleton.PlayerIds.Length;
                if (totalMovementsCompleted >= totalPlayers) {
                    totalMovementsCompleted = 0;
                    // All players have moved at least once, start minigame
                    //GameManager.Singleton.StartMinigame("MgGarbage");
                    //return;
                }
            }

            NextPlayerTurn();
        }
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
        
        if (CurrentTurnManager.Instance != null) {
            CurrentTurnManager.Instance.UpdateCurrentPlayerName(newPlayer.PlayerName);
            CurrentTurnManager.Instance.AllowRollDiceButtonFor(newPlayerId);
        }
        Debug.Log($"Current player changed to {newPlayerId}");
    }

    public void OnStateChanged(State oldState, State newState) {
        Debug.Log($"State changed from {oldState} to {newState}");
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (currentState != State.PLAYER_TURN) {
            Debug.Log("Not player turn");
            return false;
        }

        bool isTurn = player.PlayerId == currentPlayerId;
        Debug.Log($"IsPlayerTurn: Player ID = {player.PlayerId}, Current player ID = {currentPlayerId}, Result = {isTurn}");
        return isTurn;
    }

    public BoardPlayer GetCurrentPlayer() {
        // First try to get player by current player ID
        BoardPlayer player = GetPlayerById(currentPlayerId);
        
        // If no player found with current ID, try to find any available player
        if (player == null) {
            var allPlayers = FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (allPlayers.Length > 0) {
                player = allPlayers[0];
                if (isServer) {
                    // Update current player ID to match the found player
                    currentPlayerId = player.PlayerId;
                    Debug.Log($"Updated current player ID to {currentPlayerId}");
                }
            }
        }
        
        return player;
    }

    public BoardPlayer GetLocalPlayer() {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public BoardPlayer GetPlayerById(int playerId) {
        return FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault(p => p.PlayerId == playerId);
    }

    #region Global Stat Update

    public void UpdateFundsStat(uint amount) {
        fundsStat = (uint)Mathf.Clamp(fundsStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateResourceStat(uint amount) {
        resourceStat = (uint)Mathf.Clamp(resourceStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateEconomyStat(uint amount) {
        economyStat = (uint)Mathf.Clamp(economyStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateSocietyStat(uint amount) {
        societyStat = (uint)Mathf.Clamp(societyStat + amount, 0, MAX_STATS_VALUE);
    }

    public void UpdateEnvironmentStat(uint amount) {
        environmentStat = (uint)Mathf.Clamp(environmentStat + amount, 0, MAX_STATS_VALUE);
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
}