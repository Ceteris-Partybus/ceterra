using Mirror;
using System.Linq;
using UnityEngine;

public class MinigameManager : NetworkedSingleton<MinigameManager> {
    protected override bool ShouldPersistAcrossScenes => true;

    [Header("Minigame Configuration")]
    [SerializeField]
    private MinigameDatabase minigameDatabase;

    protected override void Awake() {
        base.Awake();
        minigameDatabase?.Initialize();
    }

    public GameObject GetPlayerPrefabForScene(string sceneName) {
        return minigameDatabase?.GetPlayerPrefab(sceneName);
    }

    public MinigameData GetMinigameData(string sceneName) {
        return minigameDatabase?.GetMinigame(sceneName);
    }

    public string[] GetAllMinigameScenes() {
        return minigameDatabase?.GetAllSceneNames();
    }

    public bool IsMinigameScene(string sceneName) {
        return GetAllMinigameScenes()?.Contains(sceneName) ?? false;
    }

    [Server]
    public void StartMinigame(string sceneName) {
        var minigameData = GetMinigameData(sceneName);
        if (minigameData != null) {
            Debug.Log($"Starting minigame: {minigameData.displayName} (Scene: {sceneName})");

            LobbyManager.singleton.ServerChangeScene(sceneName);
        }
        else {
            Debug.LogError($"Minigame data not found for scene: {sceneName}");
        }
    }

    [Server]
    public void ReturnToBoardScene() {
        Debug.Log("Returning to board scene");
        // TODO: Maybe do persist boardplayer? Otherwise I'd have to spawn them again
        // TODO: And I have no idea if the boardplayer spawned by mirror is the localplayer, if so, creating them anew like I do now with
        // TODO: The minigame players might cause the BoardPlayer to malfunciton
        NetworkManager.singleton.ServerChangeScene(LobbyManager.singleton.GameplayScene);
    }
}