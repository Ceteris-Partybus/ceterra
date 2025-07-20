using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance;

    [Header("Game Settings")]
    public int minPlayersToStart = 2;
    // public List<Transform> boardFields = new List<Transform>();

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

    void Start() {
        // if (boardFields.Count == 0) {
        //     GameObject[] fields = GameObject.FindGameObjectsWithTag("BoardField");
        //     foreach (GameObject field in fields) {
        //         boardFields.Add(field.transform);
        //     }
        //     boardFields.Sort((a, b) => a.name.CompareTo(b.name));
        // }

        // Debug.Log($"GameManager started. Found {boardFields.Count} board fields.");

        Invoke(nameof(FixEventSystems), 0.1f);
    }

    void FixEventSystems() {
        UnityEngine.EventSystems.EventSystem[] systems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"Found {systems.Length} EventSystems");

        for (int i = 1; i < systems.Length; i++) {
            Debug.Log($"Destroying duplicate EventSystem on: {systems[i].name}");
            Destroy(systems[i]);
        }
    }

    [Server]
    public void RegisterPlayer(BoardPlayer player) {
        Debug.Log($"Registering player: {player.playerName} (Total before: {players.Count})");

        if (!players.Contains(player)) {
            players.Add(player);
            connectedPlayers = players.Count;

            // if (boardFields.Count > 0) {
            //     // player.transform.position = boardFields[0].position + Vector3.up;
            // }
            player.transform.position = fieldList.Head.Position;
            player.currentSplineKnotIndex = fieldList.Head.SplineKnotIndex;

            Debug.Log($"Player registered! Total players: {connectedPlayers}");

            if (connectedPlayers >= minPlayersToStart && gameState == GameState.WaitingForPlayers) {
                Debug.Log("Starting game!");
                StartGame();
            }
        }
        else {
            Debug.Log("Player already registered!");
        }
    }

    [Server]
    public void UnregisterPlayer(BoardPlayer player) {
        Debug.Log($"Unregistering player: {player.playerName}");

        if (players.Remove(player)) {
            connectedPlayers = players.Count;
            Debug.Log($"Player unregistered! Total players: {connectedPlayers}");

            if (connectedPlayers < minPlayersToStart) {
                gameState = GameState.WaitingForPlayers;
                Debug.Log("Not enough players, waiting for more...");
            }
        }
    }

    [Server]
    void StartGame() {
        gameState = GameState.GameStarted;
        currentPlayerIndex = 0;
        RpcGameStarted();

        Debug.Log("Game started! Starting first player's turn...");

        Invoke(nameof(StartPlayerTurn), 1f);
    }

    [Server]
    void StartPlayerTurn() {
        if (players.Count > 0 && currentPlayerIndex < players.Count) {
            gameState = GameState.PlayerTurn;
            currentPlayerName = players[currentPlayerIndex].playerName;
            Debug.Log($"It's {players[currentPlayerIndex].playerName}'s turn!");
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

    float CalculateMovementTime(int startField, int endField) {
        int fieldsToMove = endField - startField;
        float timePerField = 1f / 3f;
        float pauseTime = 0.3f;

        return (fieldsToMove * timePerField) + ((fieldsToMove - 1) * pauseTime);
    }

    [Server]
    void NextPlayerTurn() {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log($"Next turn: Player {currentPlayerIndex}");
        StartPlayerTurn();
    }

    [ClientRpc]
    void RpcGameStarted() {
        Debug.Log("Game Started! Players can now take turns.");
    }

    [ClientRpc]
    void RpcNotifyPlayerTurn(int playerIndex, string playerName) {
        Debug.Log($"It's {playerName}'s turn! (Player {playerIndex})");
    }

    void OnCurrentPlayerChanged(int oldValue, int newValue) {
        Debug.Log($"Current player changed from {oldValue} to {newValue}");
    }

    void OnGameStateChanged(GameState oldState, GameState newState) {
        Debug.Log($"Game state changed from {oldState} to {newState}");
    }

    public bool IsPlayerTurn(BoardPlayer player) {
        if (gameState != GameState.PlayerTurn) { return false; }

        if (isServer && players.Count > 0 && currentPlayerIndex < players.Count) {
            return players[currentPlayerIndex] == player;
        }

        return player.playerName == currentPlayerName;
    }
}