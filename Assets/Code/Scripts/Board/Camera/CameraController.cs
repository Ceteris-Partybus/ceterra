using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 8, -6); // Middle-height birds-eye view offset
    public float followSpeed = 3f; // Smooth transition speed
    public float rotationSpeed = 2f; // How fast the camera rotates to look at player

    [Header("Auto-Detection")]
    public bool autoFindCamera = true; // Automatically find main camera if not set

    private Camera targetCamera;
    private Transform currentTargetPlayer;
    private bool isTransitioning = false;

    void Start() {
        // Auto-find the main camera if not set and auto-detection is enabled
        if (autoFindCamera && targetCamera == null) {
            targetCamera = Camera.main;
            if (targetCamera == null) {
                Debug.LogWarning("CameraController: No main camera found. Please assign a camera manually.");
                return;
            }
        }

        // Initial setup - position camera at a default location if no current player
        SetupInitialPosition();
    }

    void LateUpdate() {
        if (targetCamera == null) {
            return;
        }

        // Get the current player from GameManager
        BoardPlayer currentPlayer = GetCurrentPlayer();

        // Check if we need to change target
        if (currentPlayer != null && (currentTargetPlayer == null || currentTargetPlayer != currentPlayer.transform)) {
            SetNewTarget(currentPlayer.transform);
        }

        // Follow the current target
        if (currentTargetPlayer != null) {
            FollowTarget();
        }
    }

    private BoardPlayer GetCurrentPlayer() {
        if (BoardContext.Instance == null) {
            return null;
        }

        // Only follow if game is in appropriate state
        if (BoardContext.Instance.CurrentState != BoardContext.State.PLAYER_TURN &&
            BoardContext.Instance.CurrentState != BoardContext.State.PLAYER_MOVING) {
            return null;
        }

        return BoardContext.Instance.GetCurrentPlayer();
    }

    void SetNewTarget(Transform newTarget) {
        currentTargetPlayer = newTarget;
        isTransitioning = true;
    }

    void FollowTarget() {
        if (currentTargetPlayer == null) {
            return;
        }

        // Calculate target position with offset
        Vector3 targetPosition = currentTargetPlayer.position + offset;

        // Smoothly move camera to target position
        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Calculate look-at direction for bird's eye view
        Vector3 lookDirection = currentTargetPlayer.position - targetCamera.transform.position;

        if (lookDirection != Vector3.zero) {
            // Create rotation that looks down at the player at an angle
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            targetCamera.transform.rotation = Quaternion.Lerp(
                targetCamera.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Check if transition is complete
        if (isTransitioning) {
            float distance = Vector3.Distance(targetCamera.transform.position, targetPosition);
            if (distance < 0.1f) {
                isTransitioning = false;
            }
        }
    }

    void SetupInitialPosition() {
        if (targetCamera == null) {
            return;
        }

        // Set a default position if no player is available yet
        targetCamera.transform.position = new Vector3(0, 10, -8);
        targetCamera.transform.rotation = Quaternion.Euler(45f, 0, 0);
    }

    // Public methods for external control
    public void SetCamera(Camera camera) {
        targetCamera = camera;
    }

    public void SetOffset(Vector3 newOffset) {
        offset = newOffset;
    }

    public void SetFollowSpeed(float speed) {
        followSpeed = speed;
    }
}
