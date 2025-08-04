using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MinigameData {
    [Scene]
    public string sceneName;
    public GameObject playerPrefab;
    public string displayName;
}

[CreateAssetMenu(fileName = "MinigameDatabase", menuName = "Game/Minigame Database")]
public class MinigameDatabase : ScriptableObject {
    [SerializeField]
    private MinigameData[] minigames;

    private Dictionary<string, MinigameData> minigameDict;

    public void Initialize() {
        minigameDict = new Dictionary<string, MinigameData>();
        foreach (var minigame in minigames) {
            minigameDict[minigame.sceneName] = minigame;
        }
    }

    public MinigameData GetMinigame(string sceneName) {
        return minigameDict.TryGetValue(sceneName, out var data) ? data : null;
    }

    public GameObject GetPlayerPrefab(string sceneName) {
        return GetMinigame(sceneName)?.playerPrefab;
    }

    public string[] GetAllSceneNames() {
        return minigames.Select(m => m.sceneName).ToArray();
    }
}