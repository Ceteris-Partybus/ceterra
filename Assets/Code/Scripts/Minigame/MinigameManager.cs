using Mirror;
using UnityEngine;

public class MinigameManager : NetworkedSingleton<MinigameManager> {
    protected override bool ShouldPersistAcrossScenes {
        get {
            return true;
        }
    }

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

    [Server]
    public void StartMinigame(string sceneName) {
        var minigameData = GetMinigameData(sceneName);
        if (minigameData != null) {
            Debug.Log($"Starting minigame: {minigameData.displayName} (Scene: {sceneName})");

            // Change to the minigame scene
            LobbyManager.singleton.ServerChangeScene(sceneName);
        }
        else {
            Debug.LogError($"Minigame data not found for scene: {sceneName}");
        }
    }

    [Server]
    public void ReturnToBoardScene() {
        Debug.Log("Returning to board scene");
        NetworkManager.singleton.ServerChangeScene(LobbyManager.singleton.GameplayScene);
    }
}