using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct WeightedTrashPrefab {
    public GameObject prefab;
    [Tooltip("Higher weight means more frequent spawning.")]
    [Range(0.1f, 10f)]
    public float weight;
}

public class MgOceanContext : MgContext<MgOceanContext, MgOceanPlayer> {
    [SerializeField]
    private GameObject spawnAreaHolder;
    [SerializeField]
    private WeightedTrashPrefab[] weightedTrashPrefabs;

    [SerializeField]
    private float initialSpawnInterval = 3f;
    [SerializeField]
    private float minSpawnInterval = 0.75f;
    [SerializeField]
    private float spawnAcceleration = 0.98f;
    [SerializeField]
    private float gameDuration = 60f;
    [SerializeField] private float scoreboardDuration = 10f;

    [SerializeField]
    [Tooltip("Z position for spawning players and trash objects.")]
    private float spawnDepth = -6f;

    private float countdownTimer;
    private Bounds spawnArea;

    protected override void Start() {
        base.Start();
    }

    public override void OnStartGame() {
        // Calculate spawn area from the spawnAreaHolder's Collider2D
        if (spawnAreaHolder != null) {
            Collider2D collider = spawnAreaHolder.GetComponent<Collider2D>();
            if (collider != null) {
                spawnArea = collider.bounds;
                Debug.Log($"[MgOceanContext] Spawn area set from Collider2D: min={spawnArea.min}, max={spawnArea.max}, size={spawnArea.size}");
            }
            else {
                Debug.LogError("[MgOceanContext] spawnAreaHolder has no Collider2D component!");
            }
        }
        if (isServer) {
            StartCoroutine(SpawnTrashRoutine());
        }

        CreateBoundaries();
        if (isClient) {
            GetLocalPlayer().CmdSpawnPlayer();
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

    private void CreateBoundaries() {
        if (spawnArea.size == Vector3.zero) {
            Debug.LogError("[MgOceanContext] Cannot create boundaries, spawnArea is not set.");
            return;
        }

        GameObject boundaryHolder = new GameObject("Boundaries");
        boundaryHolder.transform.SetParent(transform);

        // Boundary thickness
        float thickness = 1f;

        // Top
        CreateBoundary("TopBoundary",
            new Vector3(spawnArea.center.x, spawnArea.max.y + thickness / 2, 0),
            new Vector2(spawnArea.size.x, thickness),
            boundaryHolder.transform);

        // Bottom
        CreateBoundary("BottomBoundary",
            new Vector3(spawnArea.center.x, spawnArea.min.y - thickness / 2, 0),
            new Vector2(spawnArea.size.x, thickness),
            boundaryHolder.transform);

        // Left
        CreateBoundary("LeftBoundary",
            new Vector3(spawnArea.min.x - thickness / 2, spawnArea.center.y, 0),
            new Vector2(thickness, spawnArea.size.y),
            boundaryHolder.transform);

        // Right
        CreateBoundary("RightBoundary",
            new Vector3(spawnArea.max.x + thickness / 2, spawnArea.center.y, 0),
            new Vector2(thickness, spawnArea.size.y),
            boundaryHolder.transform);
    }

    private void CreateBoundary(string name, Vector3 position, Vector2 size, Transform parent) {
        GameObject boundary = new GameObject(name);
        boundary.transform.SetParent(parent);
        boundary.transform.position = position;
        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private IEnumerator SpawnTrashRoutine() {
        float startTime = Time.time;
        float interval = initialSpawnInterval;

        float totalWeight = weightedTrashPrefabs.Sum(p => p.weight);

        while (true) {
            float elapsed = Time.time - startTime;
            if (elapsed >= gameDuration) {
                break;
            }

            GameObject prefab = null;
            float randomWeight = Random.Range(0, totalWeight);
            float currentWeight = 0;

            foreach (var weightedPrefab in weightedTrashPrefabs) {
                currentWeight += weightedPrefab.weight;
                if (randomWeight <= currentWeight) {
                    prefab = weightedPrefab.prefab;
                    break;
                }
            }

            if (prefab == null) {
                Debug.LogWarning("[MgOceanContext] No prefab selected for spawning, check weights.");
                yield return new WaitForSeconds(interval);
                continue;
            }

            bool spawnFromRight = Random.value > 0.5f;
            // Spawn outside the visible area so they swim in
            float spawnX = spawnFromRight ? spawnArea.max.x + 1f : spawnArea.min.x - 1f;

            Vector3 spawnPosition = new Vector3(
                spawnX,
                Random.Range(spawnArea.min.y, spawnArea.max.y),
                spawnDepth
            );

            Debug.Log($"[MgOceanContext] Spawning trash at {spawnPosition}, direction: {(spawnFromRight ? "left" : "right")}");

            GameObject go = Instantiate(prefab, spawnPosition, Quaternion.identity);

            var trash = go.GetComponent<MgOceanTrash>();
            if (trash != null) {
                trash.SetMovementDirection(spawnFromRight ? Vector2.left : Vector2.right);
                // flip the sprite if spawning from the left
                trash.isFlipped = !spawnFromRight;
            }
            else {
                Debug.LogWarning($"[MgOceanContext] Spawned prefab {prefab.name} has no MgOceanTrash component!");
            }

            NetworkServer.Spawn(go);

            float remaining = Mathf.Max(0f, gameDuration - (Time.time - startTime));
            if (remaining <= 0f) {
                break;
            }

            float waitTime = Mathf.Min(interval, remaining);
            yield return new WaitForSeconds(waitTime);

            interval = Mathf.Max(minSpawnInterval, interval * spawnAcceleration);
        }

        MgRewardService.Instance.DistributeRewards();
        yield return new WaitForSeconds(scoreboardDuration);

        GameManager.Singleton.EndMinigame();
    }

    public MgOceanPlayer GetLocalPlayer() {
        return FindObjectsByType<MgOceanPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public Vector3 GetPlayerSpawnPosition() {
        if (spawnArea.size != Vector3.zero) {
            return new Vector3(0f, spawnArea.min.y + 1f, spawnDepth);
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null) {
            float spawnY = -mainCamera.orthographicSize * 0.5f;
            return new Vector3(0f, spawnY, spawnDepth);
        }

        return new Vector3(0f, 0f, spawnDepth);
    }

    public Bounds GetPlayAreaBounds() {
        return spawnArea;
    }
}