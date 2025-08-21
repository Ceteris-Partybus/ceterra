using Mirror;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkRoomManager {
    public static new GameManager Singleton {
        get {
            return NetworkManager.singleton as GameManager;
        }
    }

    [SerializeField]
    private string[] minigameScenes;
    public string[] MinigameScenes => minigameScenes;

    public int[] PlayerIds => roomSlots.Select(slot => (int)slot.netId).ToArray();

    public override void OnRoomServerSceneChanged(string sceneName) {
        Debug.Log($"[Server] Scene changed to {sceneName}");

        // Handle all scene conditional players
        foreach (var player in FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            player.HandleSceneChange(sceneName);
        }

        if (sceneName == GameplayScene) {
            BoardContext.Instance?.StartPlayerTurn();
        }
    }

    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();
        if (networkSceneName != RoomScene) {
            foreach (var player in FindObjectsByType<LobbyPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                player.Hide();
            }
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        var lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();

        // Set player data on all scene conditional components
        foreach (var scenePlayer in gamePlayer.GetComponents<SceneConditionalPlayer>()) {
            scenePlayer.SetPlayerData(lobbyPlayer.Id, lobbyPlayer.PlayerName);
        }

        return true;
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
}