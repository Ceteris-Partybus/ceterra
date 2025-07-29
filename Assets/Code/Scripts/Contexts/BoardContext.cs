using Mirror;
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
        get {
            return fieldList;
        }

        set {
            fieldList ??= value;
        }
    }

    // Property to access GameManager's players list
    private SyncList<Player> Players => GameManager.Instance?.Players;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    [SerializeField]
    private int currentPlayerIndex = 0;
    public int CurrentPlayerIndex => currentPlayerIndex;

    [SyncVar]
    [SerializeField]
    private string currentPlayerName = "";
    public string CurrentPlayerName => currentPlayerName;

    [Server]
    public void StartPlayerTurn() {
        if (Players == null || currentPlayerIndex >= Players.Count) {
            return;
        }

        currentState = State.PLAYER_TURN;
        currentPlayerName = Players[currentPlayerIndex].playerName;
        RpcNotifyPlayerTurn(currentPlayerIndex, currentPlayerName);
    }

    [Server]
    public void ProcessDiceRoll(Player player, int diceValue) {
        if (currentState != State.PLAYER_TURN) { return; }

        if (Players == null || currentPlayerIndex >= Players.Count || Players[currentPlayerIndex] != player) { return; }

        currentState = State.PLAYER_MOVING;
        player.MoveToField(diceValue);
    }

    [Server]
    public void NextPlayerTurn() {
        if (Players == null || Players.Count == 0) {
            return;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
        StartPlayerTurn();
    }

    [Server]
    public void OnPlayerMovementComplete(Player player) {
        // Only proceed if the player who finished moving is the current player
        if (currentState == State.PLAYER_MOVING &&
            Players != null &&
            currentPlayerIndex < Players.Count &&
            Players[currentPlayerIndex] == player) {
            NextPlayerTurn();
        }
    }

    [ClientRpc]
    public void RpcNotifyPlayerTurn(int playerIndex, string playerName) {
    }

    public void OnCurrentPlayerChanged(int oldValue, int newValue) {

    }

    public void OnStateChanged(State oldState, State newState) {
    }

    public bool IsPlayerTurn(Player player) {
        if (currentState != State.PLAYER_TURN) { return false; }

        if (isServer && Players != null && Players.Count > 0 && currentPlayerIndex < Players.Count) {
            return Players[currentPlayerIndex] == player;
        }

        return player.playerName == currentPlayerName;
    }

    public Player GetCurrentPlayer() {
        if (Players != null && Players.Count > 0 && currentPlayerIndex >= 0 && currentPlayerIndex < Players.Count) {
            return Players[currentPlayerIndex];
        }
        return null;
    }
}