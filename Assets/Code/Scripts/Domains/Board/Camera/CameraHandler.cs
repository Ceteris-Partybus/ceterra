using Unity.Cinemachine;
using UnityEngine;
using Mirror;
using System;
using System.Collections;
using DG.Tweening;

public class CameraHandler : NetworkedSingleton<CameraHandler> {
    [Header("References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera boardCamera;
    private CinemachineBrain cameraBrain;

    [Header("States")]
    public bool IsShowingBoard => boardCamera.Priority == 1;
    public bool IsZoomedIn => zoomCamera.Priority == 1;
    private bool wasZoomedBeforeBoard = false;
    public Transform ZoomTarget => zoomCamera.Follow;
    [SyncVar] private bool hasReachedTarget = true;
    public bool HasReachedTarget {
        get => hasReachedTarget;
        set => hasReachedTarget = value;
    }

    private float playerToZoomBlendTime;
    public float PlayerToZoomBlendTime => playerToZoomBlendTime;
    private float zoomToPlayerBlendTime;
    public float ZoomToPlayerBlendTime => zoomToPlayerBlendTime;
    private float playerToBoardOverviewBlendTime;
    public float PlayerToBoardOverviewBlendTime => playerToBoardOverviewBlendTime;
    private float boardOverviewToPlayerBlendTime;
    public float BoardOverviewToPlayerBlendTime => boardOverviewToPlayerBlendTime;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float shakeIntensity;
    private CinemachineBasicMultiChannelPerlin cameraToShakeNoise;

    protected override void Start() {
        base.Start();
        SetupInitialPosition();
        cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
        var blendSettings = cameraBrain.CustomBlends;
        var defaultBlend = cameraBrain.DefaultBlend;
        var getBlendTimeFor = new Func<CinemachineCamera, CinemachineCamera, float>((CinemachineCamera from, CinemachineCamera to) => CinemachineBlenderSettings.LookupBlend(from, to, defaultBlend, blendSettings, null).BlendTime);
        playerToZoomBlendTime = getBlendTimeFor(defaultCamera, zoomCamera);
        zoomToPlayerBlendTime = getBlendTimeFor(zoomCamera, defaultCamera);
        playerToBoardOverviewBlendTime = getBlendTimeFor(defaultCamera, boardCamera);
        boardOverviewToPlayerBlendTime = getBlendTimeFor(boardCamera, defaultCamera);
    }

    [ClientRpc]
    public void RpcZoomIn() {
        ZoomIn();
    }

    [ClientRpc]
    public void RpcZoomOut() {
        ZoomOut();
    }

    [ClientRpc]
    public void RpcSwitchZoomTarget(BoardPlayer player) {
        if (!IsZoomedIn) { return; }
        zoomCamera.Follow = player.transform;
        StartCoroutine(WaitForCameraMovementAfterTargetSwitch());
    }

    [Command(requiresAuthority = false)]
    public void CmdHasReachedTarget(bool reached) {
        hasReachedTarget = reached;
    }

    [ClientRpc]
    public void RpcToggleBoardOverview() {
        if (!IsShowingBoard) {
            wasZoomedBeforeBoard = zoomCamera.Priority == 1;

            defaultCamera.Priority = -1;
            zoomCamera.Priority = -1;
            boardCamera.Priority = 1;
        }
        else {
            defaultCamera.Priority = wasZoomedBeforeBoard ? -1 : 1;
            zoomCamera.Priority = wasZoomedBeforeBoard ? 1 : -1;
            boardCamera.Priority = -1;
        }
    }

    [Client]
    public WaitWhile ZoomIn() {
        ZoomCamera(true);
        return new WaitWhile(() => cameraBrain.IsLiveInBlend(zoomCamera));
    }

    [Client]
    public WaitWhile ZoomOut() {
        ZoomCamera(false);
        return new WaitWhile(() => cameraBrain.IsLiveInBlend(zoomCamera));
    }

    [ClientRpc]
    public void RpcShakeCamera(float duration, float intensity) {
        var cameraToShake = cameraBrain.ActiveVirtualCamera as CinemachineCamera;
        cameraToShakeNoise = cameraToShake.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
        shakeTimer = duration;
        shakeTimerTotal = duration;
        shakeIntensity = intensity;
    }

    private void ZoomCamera(bool zoom) {
        defaultCamera.Priority = zoom ? -1 : 1;
        zoomCamera.Priority = zoom ? 1 : -1;
        boardCamera.Priority = -1;
    }

    private void SetupInitialPosition() {
        Follow(new GameObject().transform);
    }

    private void LateUpdate() {
        var currentPlayer = BoardContext.Instance.GetCurrentPlayer();
        if (currentPlayer != null && currentPlayer.transform != defaultCamera.Follow) {
            Follow(currentPlayer.transform);
        }
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
            cameraToShakeNoise.AmplitudeGain = Mathf.Lerp(shakeIntensity, 0f, 1 - shakeTimer / shakeTimerTotal);
        }
    }

    private void Follow(Transform trackingTarget) {
        defaultCamera.Follow = trackingTarget;
        zoomCamera.Follow = trackingTarget;
    }

    private IEnumerator WaitForCameraMovementAfterTargetSwitch() {
        var startPosition = Camera.main.transform.position;
        yield return new WaitUntil(() => startPosition != Camera.main.transform.position);

        var lastPosition = Camera.main.transform.position;
        while (true) {
            yield return new WaitForSeconds(0.1f);
            var hasStopped = lastPosition == Camera.main.transform.position;
            if (hasStopped) { break; }
            lastPosition = Camera.main.transform.position;
        }
        CmdHasReachedTarget(true);
    }
}
