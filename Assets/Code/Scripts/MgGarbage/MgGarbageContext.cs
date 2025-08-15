using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MgGarbageContext : NetworkedSingleton<MgGarbageContext> {
    [SerializeField]
    private GameObject spawnPointsHolder;
    [SerializeField]
    private GameObject[] trashPrefabs;
    [SerializeField]
    private GameObject destructionLine;
    public GameObject DestructionLine => destructionLine;
    [SerializeField]
    private GameObject binsHolder;
    public GameObject BinsHolder => binsHolder;

    [SerializeField]
    private float initialSpawnInterval = 10.0f;
    [SerializeField]
    private float minSpawnInterval = 0.1f;
    [SerializeField]
    private float spawnAcceleration = 0.1f;

    protected override void Start() {
        base.Start();
        if (isServer) {
            StartCoroutine(SpawnTrashRoutine());
        }
    }

    private IEnumerator SpawnTrashRoutine() {
        float timer = 0f;
        float duration = 180f;
        float interval = initialSpawnInterval;

        Transform[] spawnPoints = new Transform[spawnPointsHolder.transform.childCount];
        for (int i = 0; i < spawnPoints.Length; i++) {
            spawnPoints[i] = spawnPointsHolder.transform.GetChild(i);
        }

        while (timer < duration) {
            // Pick random prefab and spawn point
            GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            NetworkServer.Spawn(go);

            yield return new WaitForSeconds(interval);
            timer += interval;

            // Accelerate spawn rate
            interval = Mathf.Max(minSpawnInterval, interval * spawnAcceleration);
        }

        GameManager.singleton.EndMinigame();
    }

    public MgGarbagePlayer GetLocalPlayer() {
        return FindObjectsByType<MgGarbagePlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }
}