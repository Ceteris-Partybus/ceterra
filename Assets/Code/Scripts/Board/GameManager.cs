using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance;

    [Header("Game Settings")]
    public int minPlayersToStart = 2;

    public FieldList fieldList;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    public int currentPlayerIndex = 0;

    [SyncVar(hook = nameof(OnGameStateChanged))]
    public GameState gameState = GameState.WaitingForPlayers;

    [SyncVar]
    public int connectedPlayers = 0;

    [SyncVar]
    public string currentPlayerName = "";

    private List<BoardPlayer> players = new List<BoardPlayer>();

    public enum GameState {
        WaitingForPlayers,
        GameStarted,
        PlayerTurn,
        PlayerMoving,
        GameEnded
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    [Server]
    public void RegisterPlayer(BoardPlayer player) {

        if (!players.Contains(player)) {
            players.Add(player);
            connectedPlayers = players.Count;

            var startPosition = fieldList.Head.Position;
            startPosition.y += 1f;
            player.transform.position = startPosition;
            player.currentSplineKnotIndex = fieldList.Head.SplineKnotIndex;

            if (connectedPlayers >= minPlayersToStart && gameState == GameState.WaitingForPlayers) {
                StartGame();
            }
        }
    }

    [Server]
    public void UnregisterPlayer(BoardPlayer player) {

        if (players.Remove(player)) {
            connectedPlayers = players.Count;

            if (connectedPlayers < minPlayersToStart) {
                gameState = GameState.WaitingForPlayers;
            }
        }
    }

    [Server]
    void StartGame() {
        gameState = GameState.GameStarted;
        currentPlayerIndex = 0;
        RpcGameStarted();

        Invoke(nameof(StartPlayerTurn), 1f);
    }

    [Server]
    void StartPlayerTurn() {
        if (players.Count > 0 && currentPlayerIndex < players.Count) {
            gameState = GameState.PlayerTurn;
            currentPlayerName = players[currentPlayerIndex].playerName;
            RpcNotifyPlayerTurn(currentPlayerIndex, currentPlayerName);
        }
    }

    [Server]
    public void ProcessDiceRoll(BoardPlayer player, int diceValue) {
        if (gameState != GameState.PlayerTurn) { return; }
        if (currentPlayerIndex >= players.Count || players[currentPlayerIndex] != player) { return; }

        gameState = GameState.PlayerMoving;
        player.MoveToField(diceValue);

        Invoke(nameof(NextPlayerTurn), 2f);
    }

    [Server]
    void NextPlayerTurn() {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        StartPlayerTurn();
    }

    [ClientRpc]
    void RpcGameStarted() {
    }

    [ClientRpc]
    void RpcNotifyPlayerTurn(int playerIndex, string playerName) {
    }

    void OnCurrentPlayerChanged(int oldValue, int newValue) {
    }

    void OnGameStateChanged(GameState oldState, GameState newState) {
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (gameState != GameState.PlayerTurn) { return false; }

        if (isServer && players.Count > 0 && currentPlayerIndex < players.Count) {
            return players[currentPlayerIndex] == player;
        }

        return player.playerName == currentPlayerName;
    }
}