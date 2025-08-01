using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardContext : NetworkedSingleton<BoardContext> {
    protected override bool ShouldPersistAcrossScenes {
        get {
            return true;
        }
    }

    public enum State {
        PLAYER_TURN,
        PLAYER_MOVING,
        PAUSED
    }

    private State currentState = State.PLAYER_TURN;
    public State CurrentState => currentState;

    private FieldList fieldList;
    public FieldList FieldList {
        get {
            return fieldList;
        }

        set {
            fieldList ??= value;
        }
    }

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    [SerializeField]
    private int currentPlayerId = 0;
    public int CurrentPlayerId => currentPlayerId;

    protected override void Start() {
        base.Start();
        currentPlayerId = LobbyManager.singleton.GetPlayerIds()[0];
    }

    [Server]
    public void StartPlayerTurn() {
        currentState = State.PLAYER_TURN;
        RpcNotifyPlayerTurn(currentPlayerId);
    }

    private BoardPlayer GetBoardPlayerById(int playerId) {
        // Debug.Log($"Looking for player with ID: {playerId}. Current player id is {currentPlayerId}");
        var spawnedObjects = NetworkServer.active ? NetworkServer.spawned : NetworkClient.spawned;

        foreach (var identity in spawnedObjects.Values) {
            if (identity.TryGetComponent<BoardPlayer>(out var boardPlayer)) {
                // Debug.Log($"Found a player with id {boardPlayer.Id}");
                if (boardPlayer.Id == playerId) {
                    return boardPlayer;
                }
            }
        }
        return null;
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
        CurrentTurnManager.Instance.UpdateCurrentPlayerName(GetBoardPlayerById(newPlayerId)?.PlayerName);
        CurrentTurnManager.Instance.AllowRollDiceButtonFor(newPlayerId);
    }

    public void OnStateChanged(State oldState, State newState) {
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (currentState != State.PLAYER_TURN) { return false; }

        return player.Id == currentPlayerId;
    }

    public BoardPlayer GetCurrentPlayer() {
        return GetBoardPlayerById(currentPlayerId);
    }

    public BoardPlayer GetLocalBoardPlayer() {
        // Find the BoardPlayer that belongs to the local client
        var spawnedObjects = NetworkServer.active ? NetworkServer.spawned : NetworkClient.spawned;

        foreach (var identity in spawnedObjects.Values) {
            if (identity.TryGetComponent<BoardPlayer>(out var boardPlayer)) {
                // Check if this is the local player's object
                if (identity.isLocalPlayer) {
                    return boardPlayer;
                }
            }
        }
        return null;
    }
}