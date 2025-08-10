using Mirror;
using UnityEngine;

public class MinigameOnePlayer : SceneConditionalPlayer {
    [SyncVar]
    private int score;
    public int Score => score;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private NetworkTransformUnreliable networkTransform;

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MinigameOne";
    }

    [Server]
    protected override void OnServerInitialize() {
        Debug.Log($"[Server] MinigameOnePlayer {name} initialized");
        score = 0;
        // Don't call RPC here - the component might still be disabled on clients
    }

    [Server]
    protected override void OnServerCleanup() {
        Debug.Log($"[Server] MinigameOnePlayer {name} cleaned up");
        // Send RPC to disable components before the SyncVar changes
        if (enabled) {
            RpcSetComponents(false);
        }
    }

    [Server]
    protected override void OnServerReceiveData(SceneConditionalPlayer source) {
        Debug.Log($"[Server] MinigameOnePlayer received data from {source.GetType().Name}");
        // Could receive board position, player stats, etc.
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        Debug.Log($"[Client] MinigameOnePlayer active state changed to: {isActive}");
        // Set components directly on the client when state changes
        SetComponentsEnabled(isActive);
    }

    [ClientCallback]
    void Update() {
        if (!isOwned || !IsActiveForCurrentScene) {
            return;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);
        if (movement.magnitude > 1f) {
            movement.Normalize();
        }

        characterController?.Move(movement * moveSpeed * Time.deltaTime);
    }

    // Handle component enabling/disabling locally on each client
    private void SetComponentsEnabled(bool enabled) {
        Debug.Log($"[Client] Setting components for {name} to enabled: {enabled}");

        if (characterController != null) {
            Debug.Log($"[Client] Setting CharacterController enabled: {enabled}");
            characterController.enabled = enabled;
        }

        if (networkTransform != null) {
            Debug.Log($"[Client] Setting NetworkTransformUnreliable enabled: {enabled}");
            networkTransform.enabled = enabled;
        }
    }

    [ClientRpc]
    private void RpcSetComponents(bool enabled) {
        Debug.Log($"[ClientRpc] Setting components for {name} to enabled: {enabled}");
        SetComponentsEnabled(enabled);
    }
}