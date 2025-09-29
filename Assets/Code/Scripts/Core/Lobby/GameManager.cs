using Mirror;
using System;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkRoomManager {
    public static GameManager Singleton {
        get {
            return NetworkManager.singleton as GameManager;
        }
    }
    [Header("Character Selection")]
    [SerializeField] private GameObject[] selectableCharacters;
    public int CharacterCount => selectableCharacters.Length;
    public GameObject GetCharacter(int index) => selectableCharacters[index];
    [SerializeField] private GameObject[] selectableDices;
    public int DiceCount => selectableDices.Length;
    public GameObject GetDice(int index) => selectableDices[index];

    [Header("Minigames")]
    [SerializeField]
    private string[] minigameScenes;
    public string[] MinigameScenes => minigameScenes;

    public int[] PlayerIds => roomSlots.Select(slot => slot.index).ToArray();

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
        if (networkSceneName == GameplayScene) {
            foreach (var field in FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                field.Show();
            }
        }
        else {
            foreach (var field in FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                field.Hide();
            }
        }
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
    public void StartMinigame(string sceneName) {
        if (!MinigameScenes.Contains(sceneName)) {
            Debug.LogError($"Scene {sceneName} is not a valid minigame scene.");
            return;
        }
        if (NetworkServer.active) {
            ServerChangeScene(sceneName);
        }
    }

    /// <summary>
    /// Called when the minigame ends and the game should return to the main gameplay scene.
    /// </summary>
    public void EndMinigame() {
        if (NetworkServer.active) {
            ServerChangeScene(GameplayScene);
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        base.OnServerAddPlayer(conn);
        var lobbyPlayer = conn.identity.GetComponent<LobbyPlayer>();
        var playerHud = FindFirstObjectByType<PlayerHud>();
        playerHud.lobbyPlayers.Add(lobbyPlayer);
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) {
        var disconnectingPlayer = conn.identity.GetComponent<LobbyPlayer>();

        base.OnRoomServerDisconnect(conn);

        var playerHud = FindFirstObjectByType<PlayerHud>();
        playerHud.lobbyPlayers.Remove(disconnectingPlayer);
    }
}