using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatastropheFieldBehaviour : FieldBehaviour {
    private const float SPHERE_EXPAND_DURATION = 5f;
    private const float SPHERE_CLEANUP_DURATION = 1.5f;
    private const float CAMERA_TOGGLE_DELAY = 1f;
    private const float PLAYER_DAMAGE_DELAY = 0.3f;

    private static int catastropheFieldCount = 0;
    private static readonly CatastropheType[] catastropheTypes = (CatastropheType[])System.Enum.GetValues(typeof(CatastropheType));

    [SyncVar] private bool hasBeenInvoked = false;
    [SyncVar] private CatastropheType catastropheType;

    [Header("References")]
    [SerializeField] private GameObject catastropheRadiusPrefab;

    [Header("Settings")]
    [SerializeField] private float zoomCameraSwitchTargetBlendTime = 1f;

    private int environmentEffect;
    private int healthEffect;
    private int effectRadius;

    public void Start() {
        catastropheType = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
        var effects = CatastropheTypeExtensions.GetEffects(catastropheType);
        environmentEffect = effects.Item1;
        healthEffect = effects.Item2;
        effectRadius = effects.Item3;
    }

    [Server]
    protected override void OnFieldInvoked(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player landed on a catastrophe field of type {catastropheType}.");
            hasBeenInvoked = true;
            StartCoroutine(ProcessCatastropheSequence(player));
        }
    }

    [Server]
    private IEnumerator ProcessCatastropheSequence(BoardPlayer triggeringPlayer) {
        yield return ToggleBoardOverviewAndWait();

        var radiusSphere = SpawnRadiusSphere(triggeringPlayer.transform.position);
        yield return AnimateSphereExpansion(radiusSphere);

        yield return ToggleBoardOverviewAndWait();

        var affectedPlayers = GetAffectedPlayers(triggeringPlayer);
        yield return ApplyDamageToPlayers(affectedPlayers);

        yield return EnsureCameraOnTriggeringPlayer(triggeringPlayer);

        yield return CleanupRadiusSphere(radiusSphere);

        BoardContext.Instance.UpdateEnvironmentStat(environmentEffect);
        CompleteFieldInvocation();
    }

    [Server]
    private IEnumerator ToggleBoardOverviewAndWait() {
        CameraHandler.Instance.RpcToggleBoardOverview();
        yield return new WaitForSeconds(CAMERA_TOGGLE_DELAY);
    }

    [Server]
    private GameObject SpawnRadiusSphere(Vector3 position) {
        var radiusSphere = Instantiate(catastropheRadiusPrefab, position, Quaternion.identity);
        radiusSphere.transform.localScale = Vector3.zero;

        var sphereCollider = radiusSphere.GetComponent<SphereCollider>();
        if (sphereCollider != null) {
            sphereCollider.radius = effectRadius;
            sphereCollider.isTrigger = true;
        }

        NetworkServer.Spawn(radiusSphere);
        return radiusSphere;
    }

    [Server]
    private IEnumerator AnimateSphereExpansion(GameObject radiusSphere) {
        float targetScale = effectRadius * 2f;
        yield return AnimateSphereScale(radiusSphere, 0f, targetScale, SPHERE_EXPAND_DURATION);
    }

    [Server]
    private List<BoardPlayer> GetAffectedPlayers(BoardPlayer triggeringPlayer) {
        var affectedPlayers = new List<BoardPlayer> { triggeringPlayer };
        var triggeringPosition = triggeringPlayer.transform.position;

        foreach (var currentPlayer in FindObjectsByType<BoardPlayer>(FindObjectsSortMode.None).Where(p => p != triggeringPlayer)) {
            var distance = Vector3.Distance(triggeringPosition, currentPlayer.transform.position);
            if (distance <= effectRadius) {
                Debug.LogWarning($"Player {currentPlayer.PlayerName} is affected! Distance: {distance}");
                affectedPlayers.Add(currentPlayer);
            }
        }

        return affectedPlayers;
    }

    [Server]
    private IEnumerator ApplyDamageToPlayers(List<BoardPlayer> affectedPlayers) {
        foreach (var affectedPlayer in affectedPlayers) {
            CameraHandler.Instance.RpcSwitchZoomTarget(affectedPlayer);
            yield return new WaitForSeconds(zoomCameraSwitchTargetBlendTime);

            affectedPlayer.IsAnimationFinished = false;
            affectedPlayer.RemoveHealth(healthEffect);
            yield return new WaitUntil(() => affectedPlayer.IsAnimationFinished);
            yield return new WaitForSeconds(PLAYER_DAMAGE_DELAY);
        }
    }

    [Server]
    private IEnumerator EnsureCameraOnTriggeringPlayer(BoardPlayer triggeringPlayer) {
        if (CameraHandler.Instance.ZoomTarget != triggeringPlayer.transform) {
            CameraHandler.Instance.RpcSwitchZoomTarget(triggeringPlayer);
            yield return new WaitForSeconds(zoomCameraSwitchTargetBlendTime);
        }
    }

    [Server]
    private IEnumerator CleanupRadiusSphere(GameObject radiusSphere) {
        float startScale = effectRadius * 2f;
        yield return AnimateSphereScale(radiusSphere, startScale, 0f, SPHERE_CLEANUP_DURATION);
        NetworkServer.Destroy(radiusSphere);
    }

    [Server]
    private IEnumerator AnimateSphereScale(GameObject sphere, float startScale, float endScale, float duration) {
        var elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var scale = Mathf.Lerp(startScale, endScale, elapsed / duration);
            sphere.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        sphere.transform.localScale = Vector3.one * endScale;
    }
}