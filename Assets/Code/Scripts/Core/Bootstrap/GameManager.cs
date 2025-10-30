using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkRoomManager {
    public static GameManager Singleton {
        get {
            return NetworkManager.singleton as GameManager;
        }
    }

    [Header("Scenes")]
    [SerializeField][Scene] private string endScene;

    [Header("Character Selection")]
    [SerializeField] private GameObject[] selectableCharacters;
    public GameObject[] SelectableCharacters {
        get => selectableCharacters;
        set => selectableCharacters = value;
    }

    public int CharacterCount => selectableCharacters.Length;
    public GameObject GetCharacter(int index) => selectableCharacters[index];
    [SerializeField] private GameObject[] selectableDices;
    public int DiceCount => selectableDices.Length;
    public GameObject GetDice(int index) => selectableDices[index];

    [Header("Minigames")]
    [SerializeField]
    private List<string> minigameScenes = new();
    public List<string> MinigameScenes => minigameScenes;

    [SerializeField]
    private List<string> playedMinigames = new();
    public List<string> PlayedMinigames => playedMinigames;

    [Header("Round management")]
    private int maxRounds = 10; // Every player throws the dice `n` times => `n` rounds
    public int MaxRounds => maxRounds;

    private int currentRound = 1;
    public int CurrentRound => currentRound;

    public int[] PlayerIds => roomSlots.Select(slot => slot.index).ToArray();

    [Server]
    public void IncrementRound() {
        currentRound++;
    }

    public void StopGameSwitchEndScene() {
        ServerChangeScene(endScene);
    }

    public override void OnRoomServerSceneChanged(string sceneName) {
        Debug.Log($"[Server] Scene changed to {sceneName}");

        foreach (var player in FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            player.HandleSceneChange(sceneName);
        }

        if (sceneName == GameplayScene) {
            BoardContext.Instance?.StartPlayerTurn();
        }
    }

    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();
        var boardFieldBehaviours = FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        if (networkSceneName == GameplayScene) {
            boardFieldBehaviours.ForEach(field => field.Show());
            return;
        }
        boardFieldBehaviours.ForEach(field => field.Hide());
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        var lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();

        foreach (var scenePlayer in gamePlayer.GetComponents<SceneConditionalPlayer>()) {
            scenePlayer.SetPlayerData(lobbyPlayer.index, lobbyPlayer.PlayerName);
        }

        gamePlayer.GetComponent<BoardPlayer>().ServerTransferCharacterSelection(lobbyPlayer);
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// Called when the server wants to start a minigame.
    /// This method will change the scene to the specified minigame scene.
    /// </summary>
    /// <param name="sceneName">The name of the minigame scene to start.</param>
    private void StartMinigame(string sceneName) {
        if (!MinigameScenes.Contains(sceneName)) {
            Debug.LogError($"Scene {sceneName} is not a valid minigame scene.");
            return;
        }

        if (playedMinigames.Count + 1 == MinigameScenes.Count) {
            playedMinigames.Clear();
        }
        else {
            if (playedMinigames.Contains(sceneName)) {
                Debug.LogError($"Scene {sceneName} has already been played in the current rotation. It should not be played again until all other minigames have been played.");
                return;
            }
        }

        playedMinigames.Add(sceneName);

        if (NetworkServer.active) {
            ServerChangeScene(sceneName);
        }
    }

    public void StartMinigame() {
        var availableMinigames = MinigameScenes.Except(playedMinigames).ToList();
        if (availableMinigames.Count == 0) {
            Debug.LogError("No available minigames to start.");
            return;
        }

        var randomIndex = UnityEngine.Random.Range(0, availableMinigames.Count);
        var selectedMinigame = availableMinigames[randomIndex];
        StartMinigame(selectedMinigame);
    }

    /// <summary>
    /// Called when the minigame ends and the game should return to the main gameplay scene.
    /// </summary>
    public void EndMinigame() {
        if (NetworkServer.active) {
            BoardContext.Instance.CurrentState = BoardContext.State.MINIGAME_FINISHED;
            ServerChangeScene(GameplayScene);
        }
    }

    #region Server Connection Logging

    [Serializable]
    private class LogData {
        public string Event;
        public string EventID;
        public string Timestamp;
        public int ConnectionId;
        public string Address;
        public int TotalPlayersAmount;
        public string PlayerName;
    }

private static readonly TimeZoneInfo berlinTimeZone = 
    TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

    private void LogConnectionEvent(string eventType, NetworkConnectionToClient conn, string playerName = "Unknown") {
        if (!NetworkServer.active) {
            return;
        }

        DateTime berlinTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, berlinTimeZone);
        string timestamp = berlinTime.ToString("yyyy-MM-dd HH:mm:ss Europe/Berlin");
        var activeConnections = NetworkServer.connections.Count;
        
        var logData = new LogData {
            Event = eventType,
            EventID = Guid.NewGuid().ToString(),
            Timestamp = timestamp,
            ConnectionId = conn.connectionId,
            Address = conn.address,
            TotalPlayersAmount = activeConnections,
            PlayerName = playerName,
        };
        
        Debug.Log(JsonUtility.ToJson(logData));
    }

    public override void OnRoomServerConnect(NetworkConnectionToClient conn) {
        base.OnRoomServerConnect(conn);
        LogConnectionEvent("ClientConnected", conn);
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) {
        string playerName = conn.identity != null ? 
            conn.identity.GetComponent<LobbyPlayer>()?.PlayerName ?? "Unknown" : 
            "Unknown";
        
        LogConnectionEvent("ClientDisconnected", conn, playerName);
        base.OnRoomServerDisconnect(conn);
    }

    #endregion

}