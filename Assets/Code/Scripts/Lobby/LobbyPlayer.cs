using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkRoomPlayer {
    [SyncVar(hook = nameof(OnHiddenStateChanged))]
    public bool isHidden = false;

    public override void OnStartClient() {
        Debug.Log($"OnStartClient {gameObject}");
    }

    public override void OnClientEnterRoom() {
        Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");
        gameObject.transform.position = LobbyManager.singleton.GetStartPosition().position;
    }

    public override void OnClientExitRoom() {
        Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
    }

    public override void IndexChanged(int oldIndex, int newIndex) {
        Debug.Log($"IndexChanged {newIndex}");
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) {
        Debug.Log($"ReadyStateChanged {newReadyState}");
    }

    void OnHiddenStateChanged(bool _, bool newHidden) {
        // Setting the scale to zero effectively hides the player in the lobby
        // Physically disabling the GameObject would disable all components of the LobbyPlayer which we might need later on
        gameObject.transform.localScale = newHidden ? Vector3.zero : Vector3.one;
    }

    [Server]
    public void SetHidden(bool hidden) {
        isHidden = hidden;
    }

#if !UNITY_SERVER
    public override void OnGUI() {
        base.OnGUI();
    }
#endif
}
