using UnityEngine;

[System.Serializable]
public class CameraSetup : MonoBehaviour {
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;

    [Header("Camera Controller Settings")]
    public Vector3 cameraOffset = new Vector3(0, 8, -6);
    public float followSpeed = 3f;
    public float rotationSpeed = 2f;

    void Start() {
        if (autoSetupOnStart) {
            SetupCameraController();
        }
    }

    [ContextMenu("Setup Camera Controller")]
    public void SetupCameraController() {
        // Check if CameraController already exists
        CameraController existingController = FindAnyObjectByType<CameraController>();
        if (existingController != null) {
            Debug.Log("CameraController already exists in the scene.");
            return;
        }

        // Find the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            Debug.LogError("No main camera found in the scene. Please ensure there's a camera tagged as 'MainCamera'.");
            return;
        }

        // Create a new GameObject for the CameraController
        GameObject cameraControllerObject = new GameObject("CameraController");
        CameraController controller = cameraControllerObject.AddComponent<CameraController>();

        // Configure the controller
        controller.SetCamera(mainCamera);
        controller.SetOffset(cameraOffset);
        controller.SetFollowSpeed(followSpeed);

        Debug.Log("CameraController setup complete!");
    }
}
