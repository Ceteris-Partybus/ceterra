using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MgOceanPlayerController3D : NetworkBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float waterDepth = 0f;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private float cameraSmoothSpeed = 5f;

    [Header("Play Area Bounds")]
    [SerializeField] private float minX = -250f;
    [SerializeField] private float maxX = 250f;
    [SerializeField] private float minZ = -100f;
    [SerializeField] private float maxZ = 100f;

    private Rigidbody rb;
    private Camera playerCamera;
    private AudioListener audioListener;
    
    private float inputForward;
    private float inputTurn;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        
        // Freeze Y position (stay on water surface) and X/Z rotation (no tipping)
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth physics between fixed updates
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        SetupCamera();
    }

    private void SetupCamera() {
        var cameraObj = new GameObject("PlayerCamera");
        playerCamera = cameraObj.AddComponent<Camera>();
        audioListener = cameraObj.AddComponent<AudioListener>();

        cameraObj.transform.position = transform.position + transform.TransformDirection(cameraOffset);
        cameraObj.transform.LookAt(transform.position);

        if (Camera.main != null && Camera.main != playerCamera) {
            Camera.main.gameObject.SetActive(false);
        }
    }

    void LateUpdate() {
        if (!isOwned || playerCamera == null) {
            return;
        }

        Vector3 targetPosition = transform.position + transform.TransformDirection(cameraOffset);
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position, 
            targetPosition, 
            cameraSmoothSpeed * Time.deltaTime
        );
        playerCamera.transform.LookAt(transform.position);
    }

    void Update() {
        if (!isOwned) {
            return;
        }

        inputForward = Input.GetAxisRaw("Vertical");
        inputTurn = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate() {
        if (!isOwned) {
            return;
        }

        ApplyMovement();
    }

    private void ApplyMovement() {
        if (Mathf.Abs(inputTurn) > 0.01f) {
            float rotationAmount = inputTurn * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAmount, 0f));
        }

        if (Mathf.Abs(inputForward) > 0.01f) {
            Vector3 movement = transform.forward * inputForward * moveSpeed * Time.fixedDeltaTime;
            Vector3 newPosition = rb.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            newPosition.y = waterDepth;

            rb.MovePosition(newPosition);
        }
    }

    void OnDestroy() {
        if (isLocalPlayer) {
            var mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCam != null) {
                mainCam.SetActive(true);
            }
        }
    }
}