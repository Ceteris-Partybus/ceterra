using Mirror;
using UnityEngine;

public class MinigameOnePlayer : MinigamePlayer {
    [Header("Minigame One Specific")]
    [SyncVar]
    [SerializeField]
    private int minigameScore = 0;
    public int MinigameScore {
        get => minigameScore;
        set => minigameScore = value;
    }

    [Header("Movement Debug")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField]
    private CharacterController characterController;

    void Start() {
    }

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

    [Command]
    void CmdIncrementScore() {
        minigameScore++;
        Debug.Log($"[SERVER] Minigame One Player {Id} score increased to {minigameScore}");
    }

    [Command]
    void CmdMove(Vector3 newPosition) {
        // Server validates and applies the movement
        Vector3 oldPos = transform.position;
        transform.position = newPosition;

        // Force sync to all clients
        transform.position = newPosition;
    }

    public override void FromBoard(BoardPlayerData boardPlayer) {
        if (boardPlayer != null) {
            // this.Id = boardPlayer.Id;
            // this.PlayerName = boardPlayer.PlayerName;
        }
        this.MinigameScore = 0;
    }

    public override void ToBoard(BoardPlayerData boardPlayer) {
        // Transfer minigame results back to board player
        // For example: boardPlayer.AddCoins(MinigameScore);
    }
}