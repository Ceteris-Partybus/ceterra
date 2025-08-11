using Mirror;
using System.Collections.Generic;
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

    private FieldList fieldList;
    public FieldList FieldList {
        get => fieldList;
        set => fieldList ??= value;
    }

    #region Global Stats
    [Header("Global Stats")]
    [SerializeField]
    private FundsStat fundsStat;
    public FundsStat FundsStat => fundsStat;

    [SerializeField]
    private ResourceStat resourceStat;
    public ResourceStat ResourceStat => resourceStat;

    [SerializeField]
    public EconomyStat economyStat;
    public EconomyStat EconomyStat => economyStat;

    [SerializeField]
    private SocietyStat societyStat;
    public SocietyStat SocietyStat => societyStat;

    [SerializeField]
    private EnvironmentStat environmentStat;
    public EnvironmentStat EnvironmentStat => environmentStat;

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
        currentPlayerId = GameManager.singleton.PlayerIds[0];

        this.fundsStat = new FundsStat(0);
        this.resourceStat = new ResourceStat(0);
        this.economyStat = new EconomyStat(50);
        this.societyStat = new SocietyStat(50);
        this.environmentStat = new EnvironmentStat(50);
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
        int[] playerIds = GameManager.singleton.PlayerIds;
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
            int totalPlayers = GameManager.singleton.PlayerIds.Length;
            if (totalMovementsCompleted >= totalPlayers) {
                totalMovementsCompleted = 0;
                // All players have moved at least once, start minigame
                GameManager.singleton.StartMinigame("MinigameOne");
                return;
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
}