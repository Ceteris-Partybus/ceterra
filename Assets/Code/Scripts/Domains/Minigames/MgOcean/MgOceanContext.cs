using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MgOceanContext : NetworkedSingleton<MgOceanContext> {
    [SerializeField]
    private GameObject spawnAreaHolder;
    [SerializeField]
    private GameObject[] trashPrefabs;
    
    [SerializeField]
    private float initialSpawnInterval = 3f;
    [SerializeField]
    private float minSpawnInterval = 0.75f;
    [SerializeField]
    private float spawnAcceleration = 0.98f;
    [SerializeField]
    private float gameDuration = 60f; 

    private float countdownTimer; 
    private Bounds spawnArea; 

    protected override void Start() {
        base.Start();

        // Calculate spawn area from the spawnAreaHolder or use camera bounds as fallback
        if (spawnAreaHolder != null) {
            Renderer renderer = spawnAreaHolder.GetComponent<Renderer>();
            if (renderer != null) {
                spawnArea = renderer.bounds;
            }
        }
        
        // If no spawn area was set, use camera bounds
        if (spawnArea.size == Vector3.zero) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                float height = 2f * mainCamera.orthographicSize;
                float width = height * mainCamera.aspect;
                spawnArea = new Bounds(mainCamera.transform.position, new Vector3(width, height, 1f));
            }
        }

        if (isServer) {
            StartCoroutine(SpawnTrashRoutine());
        }
        
        StartCoroutine(UpdateCountdown());
    }

    private IEnumerator UpdateCountdown() {
        countdownTimer = gameDuration;
        int lastSeconds = Mathf.CeilToInt(countdownTimer);
        Debug.Log($"[MgOceanContext] Starting countdown from {lastSeconds}s.");
        MgOceanLocalPlayerHUD.Instance?.UpdateCountdown(lastSeconds);

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, countdownTimer));
            if (seconds != lastSeconds) {
                MgOceanLocalPlayerHUD.Instance?.UpdateCountdown(seconds);
                lastSeconds = seconds;
            }
            yield return null;
        }

        Debug.Log("[MgOceanContext] Countdown finished.");
        MgOceanLocalPlayerHUD.Instance?.UpdateCountdown(0);
    }

    private IEnumerator SpawnTrashRoutine() {
        float startTime = Time.time;
        float interval = initialSpawnInterval;

        while (Time.time - startTime < gameDuration) {
            GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnArea.min.x, spawnArea.max.x),
                spawnArea.max.y + 1f,
                0f
            );

            GameObject go = Instantiate(prefab, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(go);

            yield return new WaitForSeconds(interval);

            interval = Mathf.Max(minSpawnInterval, interval * spawnAcceleration);
        }

        yield return new WaitForSeconds(3f);
        GameManager.Singleton.EndMinigame();
    }

    public MgOceanPlayer GetLocalPlayer() {
        return FindObjectsByType<MgOceanPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public MgOceanPlayer[] GetPlayersByScore() {
        return FindObjectsByType<MgOceanPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .OrderByDescending(p => p.Score)
            .ToArray();
    }

    public Vector3 GetPlayerSpawnPosition() {
        if (spawnArea.size != Vector3.zero) {
            return new Vector3(0f, spawnArea.min.y + 1f, 0f);
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null) {
            float spawnY = -mainCamera.orthographicSize * 0.5f;
            return new Vector3(0f, spawnY, 0f);
        }
        
        return Vector3.zero;
    }
}
