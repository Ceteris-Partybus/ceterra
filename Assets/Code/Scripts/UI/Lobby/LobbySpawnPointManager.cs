using UnityEngine;

public class LobbySpawnPointManager : NetworkedSingleton<LobbySpawnPointManager> {
    [SerializeField] private Transform[] spawnPoints;
    public Transform GetSpawnPoint(int index) => spawnPoints[index];
}
