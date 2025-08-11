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
    public const uint MAX_STATS_VALUE = 100;

    [Header("Global Stats")]
    [SerializeField]
    private uint fundsStat;
    public uint FundsStat => fundsStat;

    [SerializeField]
    private uint resourceStat;
    public uint ResourceStat => resourceStat;

    [SerializeField]
    private uint economyStat;
    public uint EconomyStat => economyStat;

    [SerializeField]
    private uint societyStat;
    public uint SocietyStat => societyStat;

    [SerializeField]
    private uint environmentStat;
    public uint EnvironmentStat => environmentStat;

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

    #endregion
}