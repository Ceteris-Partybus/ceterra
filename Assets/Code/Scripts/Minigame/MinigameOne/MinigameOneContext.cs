using Mirror;
using UnityEngine;

public class MinigameOneContext : NetworkedSingleton<MinigameOneContext> {
    protected override bool ShouldPersistAcrossScenes {
        get {
            return false; // Minigame contexts are scene-specific
        }
    }

    [Header("Minigame State")]
    [SyncVar]
    [SerializeField]
    private bool isMinigameActive = false;
    public bool IsMinigameActive => isMinigameActive;

    protected override void Start() {
        base.Start();

        if (NetworkServer.active) {
            InitializeMinigame();
        }
    }

    [Server]
    private void InitializeMinigame() {
        Debug.Log("MinigameOne initialized");
        isMinigameActive = true;

        // Add any minigame-specific initialization here
        // For example: set up minigame rules, timers, objectives, etc.
    }

    [Server]
    public void EndMinigame() {
        Debug.Log("MinigameOne ended");
        isMinigameActive = false;

        // Return to board scene
        MinigameManager.Instance?.ReturnToBoardScene();
    }
}