using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MgGarbageContext : MgContext<MgGarbageContext, MgGarbagePlayer> {
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
    private float initialSpawnInterval = 5f; // start with 5 seconds between spawns
    [SerializeField]
    private float minSpawnInterval = 1.25f; // don't go below 1.25 seconds
    [SerializeField]
    private float spawnAcceleration = 0.97f; // each step is 97% of the last delay
    [SerializeField]
    private float gameDuration = 15f;

    private float countdownTimer; // Separate timer for countdown display

    protected override void Start() {
        base.Start();
        if (isServer) {
            StartCoroutine(SpawnTrashRoutine());
        }
        StartCoroutine(UpdateCountdown());
    }

    private IEnumerator UpdateCountdown() {
        countdownTimer = gameDuration; // Initialize countdown timer
        int lastSeconds = Mathf.CeilToInt(countdownTimer);
        LocalPlayerHUD.Instance.UpdateCountdown(lastSeconds);

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, countdownTimer));
            if (seconds != lastSeconds) {
                LocalPlayerHUD.Instance.UpdateCountdown(seconds);
                lastSeconds = seconds;
            }
            yield return null;
        }

        LocalPlayerHUD.Instance.UpdateCountdown(0);
    }

    private IEnumerator SpawnTrashRoutine() {
        float startTime = Time.time;
        float interval = initialSpawnInterval;

        Transform[] spawnPoints = new Transform[spawnPointsHolder.transform.childCount];
        for (int i = 0; i < spawnPoints.Length; i++) {
            spawnPoints[i] = spawnPointsHolder.transform.GetChild(i);
        }

        while (Time.time - startTime < gameDuration) {
            // Pick random prefab and spawn point
            GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            NetworkServer.Spawn(go);

            yield return new WaitForSeconds(interval);

            // Accelerate spawn rate
            interval = Mathf.Max(minSpawnInterval, interval * spawnAcceleration);
        }

        GameManager.Singleton.EndMinigame();
    }

    public MgGarbagePlayer GetLocalPlayer() {
        return FindObjectsByType<MgGarbagePlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }
}