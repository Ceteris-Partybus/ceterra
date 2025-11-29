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
    [SerializeField] private float scoreboardDuration = 10f;

    [SyncVar]
    private bool hasStarted = false;
    public bool HasStarted => hasStarted;

    [SerializeField]
    private float countdownTimer;

    protected override void Start() {
        StartCoroutine(WaitForAllPlayers());

        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count());
        }

        base.Start();
    }

    public override void OnStartGame() {
        StartCoroutine(UpdateCountdown());
        if (isServer) {
            StartCoroutine(SpawnTrashRoutine());
            hasStarted = true;
        }
    }

    private IEnumerator UpdateCountdown() {
        countdownTimer = gameDuration;
        int lastSeconds = Mathf.CeilToInt(countdownTimer);
        MgGarbageLocalPlayerHUD.Instance.UpdateCountdown(lastSeconds);

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, countdownTimer));
            if (seconds != lastSeconds) {
                MgGarbageLocalPlayerHUD.Instance.UpdateCountdown(seconds);
                lastSeconds = seconds;
            }
            yield return null;
        }

        MgGarbageLocalPlayerHUD.Instance.UpdateCountdown(0);
    }

    private IEnumerator SpawnTrashRoutine() {
        float startTime = Time.time;
        float interval = initialSpawnInterval;

        Transform[] spawnPoints = new Transform[spawnPointsHolder.transform.childCount];
        for (int i = 0; i < spawnPoints.Length; i++) {
            spawnPoints[i] = spawnPointsHolder.transform.GetChild(i);
        }

        while (true) {
            float elapsed = Time.time - startTime;
            if (elapsed >= gameDuration) {
                break;
            }

            GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            NetworkServer.Spawn(go);

            float remaining = Mathf.Max(0f, gameDuration - (Time.time - startTime));
            if (remaining <= 0f) {
                break;
            }

            float waitTime = Mathf.Min(interval, remaining);
            yield return new WaitForSeconds(waitTime);

            interval = Mathf.Max(minSpawnInterval, interval * spawnAcceleration);
        }

        yield return new WaitForSeconds(2f); // Um die runterfallenden Objekte abzuwarten ggf Zeit anpassen
        MgRewardService.Instance.DistributeRewards();
        yield return new WaitForSeconds(scoreboardDuration);

        GameManager.Singleton.EndMinigame();
    }

    public MgGarbagePlayer GetLocalPlayer() {
        return FindObjectsByType<MgGarbagePlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }
}