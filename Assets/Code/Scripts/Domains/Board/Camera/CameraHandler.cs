using Unity.Cinemachine;
using UnityEngine;
using Mirror;

public class CameraHandler : NetworkedSingleton<CameraHandler> {
    [Header("References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera boardCamera;

    [Header("States")]
    private bool isShowingBoard = false;
    public bool IsShowingBoard => isShowingBoard;
    private bool wasZoomedBeforeBoard = false;
    public Transform ZoomTarget => zoomCamera.Follow;

    protected override void Start() {
        base.Start();
        SetupInitialPosition();
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
        if (zoomCamera.Priority != 1) { return; }
        zoomCamera.Follow = player.transform;
    }

    [ClientRpc]
    public void RpcToggleBoardOverview() {
        isShowingBoard = !isShowingBoard;

        if (isShowingBoard) {
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

    public void ZoomIn() {
        ZoomCamera(true);
    }

    public void ZoomOut() {
        ZoomCamera(false);
    }
    public void ZoomCamera(bool zoom) {
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

    public void Follow(Transform trackingTarget) {
        defaultCamera.Follow = trackingTarget;
        zoomCamera.Follow = trackingTarget;
    }
}
