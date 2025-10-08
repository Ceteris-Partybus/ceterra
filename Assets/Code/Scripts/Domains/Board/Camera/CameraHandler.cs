using Unity.Cinemachine;
using UnityEngine;
using Mirror;
using System;
using System.Collections;

public class CameraHandler : NetworkedSingleton<CameraHandler> {
    [Header("References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera boardCamera;

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

    protected override void Start() {
        base.Start();
        SetupInitialPosition();
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        var blendSettings = brain.CustomBlends;
        var defaultBlend = brain.DefaultBlend;
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
    public void ZoomIn() {
        ZoomCamera(true);
    }

    [Client]
    public void ZoomOut() {
        ZoomCamera(false);
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
