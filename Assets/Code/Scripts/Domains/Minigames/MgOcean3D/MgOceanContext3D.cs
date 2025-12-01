using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;


public class MgOceanContext3D : MgContext<MgOceanContext3D, MgOceanPlayer3D> {
    [Header("Spawn Settings")]
    [SerializeField] private BoxCollider spawnAreaCollider;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float scoreboardDuration = 10f;

    public override void OnStartGame() {
        if (isServer) {
            StartCoroutine(GameLoop());
        }

        if (isClient) {
            var localPlayer = GetLocalPlayer();
            if (localPlayer != null) {
                localPlayer.CmdSpawnBoat();
            }
        }
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