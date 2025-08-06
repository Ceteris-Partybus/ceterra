using Mirror;
using UnityEngine;

public class MinigameOnePlayer : SceneConditionalPlayer {
    [SyncVar]
    private int score;
    public int Score => score;

    public override bool IsEnabledForScene(string sceneName) {
        return sceneName == "MinigameOne";
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private NetworkTransformUnreliable networkTransform;

    [ClientCallback]
    void Update() {
        if (isOwned == false) { return; }

        // Handle movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);
        if (movement.magnitude > 1f) {
            movement.Normalize();  // Normalize to prevent faster diagonal movement
        }

        characterController.Move(movement * moveSpeed * Time.deltaTime);
    }

    protected override void Cleanup() {
        score = 0;
        RpcSetComponents(false);
    }

    [ServerCallback]
    protected override void Initialize() {
        Debug.Log("Initializing MinigameOnePlayer");
        RpcSetComponents(true);
    }

    // Ich hasse Unity
    [ClientRpc]
    private void RpcSetComponents(bool enabled) {
        Debug.Log($"Setting components enabled: {enabled} for MinigameOnePlayer {name}");
        characterController.enabled = enabled;
        networkTransform.enabled = enabled;
    }

    protected override void OnTransferDataTo(SceneConditionalPlayer enabledScript) {
        // if (enabledScript is BoardPlayer boardPlayer) {
        // boardPlayer.AddMoney(score / 2);
        // }
    }
}