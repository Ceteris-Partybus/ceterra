using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardContext : PlayerDataContext<BoardContext, BoardPlayer, BoardPlayerData> {
    protected override bool ShouldPersistAcrossScenes => true;

    public enum State {
        PLAYER_TURN,
        PLAYER_MOVING,
        PAUSED
    }

    private State currentState = State.PLAYER_TURN;
    public State CurrentState => currentState;

    private FundsDisplay fundsDispaly;
    public FundsDisplay FundsDisplay => fundsDispaly;

    private FieldList fieldList;
    public FieldList FieldList {
        get => fieldList;
        set => fieldList ??= value;
    }

    [Header("Current Player")]
    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    [SerializeField]
    private int currentPlayerId = 0;
    public int CurrentPlayerId => currentPlayerId;

    protected override void Start() {
        base.Start();
        currentPlayerId = playerIds[0];
        fundsDispaly = new FundsDisplay(0);
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

        if (currentPlayerId != player.Id) {
            return;
        }

        currentState = State.PLAYER_MOVING;
        player.MoveToField(diceValue);
    }

    [Server]
    public void NextPlayerTurn() {
        List<int> playerIds = LobbyManager.singleton.GetPlayerIds();
        int indexInLobby = playerIds.IndexOf(currentPlayerId);
        currentPlayerId = playerIds[(indexInLobby + 1) % playerIds.Count];

        StartPlayerTurn();
    }

    [Server]
    public void OnPlayerMovementComplete(BoardPlayer player) {
        if (currentState == State.PLAYER_MOVING &&
            currentPlayerId == player.Id) {
            NextPlayerTurn();
        }
    }

    [ClientRpc]
    public void RpcNotifyPlayerTurn(int playerId) {
    }

    public void OnCurrentPlayerChanged(int _, int newPlayerId) {
        CurrentTurnManager.Instance.UpdateCurrentPlayerName(GetPlayerById(newPlayerId)?.PlayerName);
        CurrentTurnManager.Instance.AllowRollDiceButtonFor(newPlayerId);
    }

    public void OnStateChanged(State oldState, State newState) {
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (currentState != State.PLAYER_TURN) { return false; }

        return player.Id == currentPlayerId;
    }

    public BoardPlayer GetCurrentPlayer() {
        return GetPlayerById(currentPlayerId);
    }
}