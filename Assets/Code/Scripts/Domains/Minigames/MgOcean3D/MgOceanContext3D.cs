using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MgOceanContext3D : MgContext<MgOceanContext3D, MgOceanPlayer3D> {
    [Header("Spawn Settings")]
    [SerializeField] private BoxCollider spawnAreaCollider;

    [Header("Trash Spawn Settings")]
    [SerializeField] private GameObject[] trashPrefabs;
    [SerializeField] private int initialTrashCount = 30;
    [SerializeField] private float trashSpawnHeight = 0f;
    [SerializeField] private float respawnInterval = 5f;
    [SerializeField] private int maxTrashCount = 50;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float scoreboardDuration = 10f;

    private List<GameObject> spawnedTrash = new List<GameObject>();

    public override void OnStartGame() {
        Debug.Log($"[MgOceanContext3D] OnStartGame called! isServer: {isServer}, isClient: {isClient}");
        
        if (isServer) {
            Debug.Log($"[MgOceanContext3D] Server: Spawning trash. Prefabs count: {trashPrefabs?.Length ?? 0}");
            SpawnInitialTrash();
            StartCoroutine(GameLoop());
            StartCoroutine(TrashRespawnLoop());
        }

        if (isClient) {
            var localPlayer = GetLocalPlayer();
            Debug.Log($"[MgOceanContext3D] Client: Local player found: {localPlayer != null}");
            if (localPlayer != null) {
                localPlayer.CmdSpawnBoat();
            }
        }
    }

    [Server]
    private void SpawnInitialTrash() {
        Debug.Log($"[MgOceanContext3D] SpawnInitialTrash: count={initialTrashCount}");
        for (int i = 0; i < initialTrashCount; i++) {
            SpawnTrash();
        }
        Debug.Log($"[MgOceanContext3D] SpawnInitialTrash complete. Spawned: {spawnedTrash.Count}");
    }

    [Server]
    private IEnumerator TrashRespawnLoop() {
        while (true) {
            yield return new WaitForSeconds(respawnInterval);

            // Clean up null references from collected trash
            spawnedTrash.RemoveAll(t => t == null);

            if (spawnedTrash.Count < maxTrashCount) {
                SpawnTrash();
            }
        }
    }

    [Server]
    private void SpawnTrash() {
        if (trashPrefabs == null || trashPrefabs.Length == 0) {
            Debug.LogError("[MgOceanContext3D] No trash prefabs assigned!");
            return;
        }

        if (spawnAreaCollider == null) {
            Debug.LogError("[MgOceanContext3D] SpawnAreaCollider is not assigned!");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;

        GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
        Debug.Log($"[MgOceanContext3D] Spawning trash prefab: {prefab.name} at {spawnPosition}");
        
        GameObject trash = Instantiate(prefab, spawnPosition, spawnRotation);

        NetworkServer.Spawn(trash);
        spawnedTrash.Add(trash);
    }

    private Vector3 GetRandomSpawnPosition() {
        Bounds bounds = spawnAreaCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, trashSpawnHeight, z);
    }

    [Server]
    private IEnumerator GameLoop() {
        yield return new WaitForSeconds(gameDuration);

        MgRewardService.Instance.DistributeRewards();

        yield return new WaitForSeconds(scoreboardDuration);

        GameManager.Singleton.EndMinigame();
    }

    public MgOceanPlayer3D GetLocalPlayer() {
        return FindObjectsByType<MgOceanPlayer3D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public Vector3 GetPlayerSpawnPosition() {
        if (spawnAreaCollider == null) {
            Debug.LogError("[MgOceanContext3D] SpawnAreaCollider is not assigned!");
            return Vector3.zero;
        }

        Bounds bounds = spawnAreaCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, 0f, z);
    }

    public Bounds GetPlayAreaBounds() {
        if (spawnAreaCollider == null) {
            Debug.LogError("[MgOceanContext3D] SpawnAreaCollider is not assigned!");
            return new Bounds();
        }

        return spawnAreaCollider.bounds;
    }
}