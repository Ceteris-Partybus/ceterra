using Mirror;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkRoomManager {
    public static new GameManager singleton => NetworkManager.singleton as GameManager;

    [SerializeField]
    private string[] minigameScenes;
    public string[] MinigameScenes => minigameScenes;

    public int[] PlayerIds => roomSlots.Select(slot => (int)slot.netId).ToArray();

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName) {
        if (sceneName == GameplayScene) {
            BoardContext.Instance.StartPlayerTurn();
        }

        foreach (var sceneConditionalPlayer in FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            sceneConditionalPlayer.Recalculate();
        }

        foreach (var player in FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            player.transform.position = GetStartPosition().position; // Only do if scene is not GameplayScene, do GamePlayscene spawn in OnRoomServerSceneLoadedForPlayer
        }
    }

    public override void OnRoomClientSceneChanged() {

    }

    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        LobbyPlayer player = roomPlayer.GetComponent<LobbyPlayer>();
        BoardPlayer boardPlayer = gamePlayer.GetComponent<BoardPlayer>();

        boardPlayer.Id = player.Id;
        boardPlayer.PlayerName = player.PlayerName;

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