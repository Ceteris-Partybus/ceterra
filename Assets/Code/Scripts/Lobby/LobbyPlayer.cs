using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkRoomPlayer {
    [SyncVar(hook = nameof(OnHiddenStateChanged))]
    [SerializeField]
    private bool isHidden = false;
    public bool IsHidden => isHidden;

    public override void OnStartClient() {
        Debug.Log($"LobbyPlayer OnStartClient {gameObject}, isHidden: {isHidden}");
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

    void OnHiddenStateChanged(bool oldHidden, bool newHidden) {
        Debug.Log($"[LobbyPlayer] OnHiddenStateChanged for player {index}: {oldHidden} -> {newHidden}");

        // Multiple approaches to hiding the player
        if (newHidden) {
            // Set scale to zero
            gameObject.transform.localScale = Vector3.zero;

            // Also disable all renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = false;
            }

            // Disable colliders to prevent interaction
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) {
                collider.enabled = false;
            }
        }
        else {
            // Restore visibility
            gameObject.transform.localScale = Vector3.one;

            // Re-enable all renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }

            // Re-enable colliders
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) {
                collider.enabled = true;
            }
        }

        Debug.Log($"[LobbyPlayer] Set scale to: {gameObject.transform.localScale}, renderers disabled: {newHidden}");
    }

    [Server]
    public void SetHidden(bool hidden) {
        Debug.Log($"[LobbyPlayer] SetHidden called for player {index}: {hidden} (current: {isHidden})");
        isHidden = hidden;
        Debug.Log($"[LobbyPlayer] isHidden is now: {isHidden}");
    }

#if !UNITY_SERVER
    public override void OnGUI() {
        base.OnGUI();
    }
#endif
}
