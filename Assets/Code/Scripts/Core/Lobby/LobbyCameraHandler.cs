using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LobbyCameraHandler : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CinemachineCamera lobbyCamera;
    [SerializeField] private CinemachineCamera characterSelectionCamera;
    [SerializeField] private float cameraBlendTime;

    [Header("States")]
    private bool isShowingLobby = false;
    public bool IsShowingLobby => isShowingLobby;

    public IEnumerator ToggleCharacterSelection() {
        isShowingLobby = !isShowingLobby;
        lobbyCamera.Priority = isShowingLobby ? -1 : 1;
        characterSelectionCamera.Priority = isShowingLobby ? 1 : -1;
        yield return new WaitForSeconds(cameraBlendTime);
    }
}
