using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;


public class MgOceanContext3D : MgContext<MgOceanContext3D, MgOceanPlayer3D> {
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnAreaHolder;
    [SerializeField] private Vector2 playAreaSize = new Vector2(50f, 30f);

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float scoreboardDuration = 10f;

    private int nextSpawnIndex = 0;

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
        if (spawnAreaHolder != null && spawnAreaHolder.childCount > 0) {
            var spawnPoint = spawnAreaHolder.GetChild(nextSpawnIndex % spawnAreaHolder.childCount);
            nextSpawnIndex++;
            return new Vector3(spawnPoint.position.x, 0f, spawnPoint.position.z);
        }

        float halfWidth = playAreaSize.x / 2f;
        float halfHeight = playAreaSize.y / 2f;

        float x = Random.Range(-halfWidth + 5f, halfWidth - 5f);
        float z = Random.Range(-halfHeight + 5f, halfHeight - 5f);

        return new Vector3(x, 0f, z);
    }
}