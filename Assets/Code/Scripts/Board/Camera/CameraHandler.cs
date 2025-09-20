using Unity.Cinemachine;
using UnityEngine;

public class CameraHandler : NetworkedSingleton<CameraHandler> {
    [Header("References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera boardCamera;

    [Header("States")]
    private bool isShowingBoard = false;
    public bool IsShowingBoard => isShowingBoard;

    protected override void Start() {
        base.Start();
        SetupInitialPosition();
    }

    public void OnRollStart() {
        ZoomCamera(true);
    }

    public void OnRollEnd() {
        ZoomCamera(false);
    }

    public void ToggleBoardOverview() {
        isShowingBoard = !isShowingBoard;
        defaultCamera.Priority = isShowingBoard ? -1 : 1;
        boardCamera.Priority = isShowingBoard ? 1 : -1;
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
