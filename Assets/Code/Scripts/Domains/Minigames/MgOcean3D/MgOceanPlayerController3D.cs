using Mirror;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Rigidbody))]
public class MgOceanPlayerController3D : NetworkBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float waterDepth = 0f;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 30f, -55f);
    [SerializeField] private float cameraSmoothSpeed = 5f;

    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;
    private bool boundsInitialized = false;

    private Rigidbody rb;
    private Camera playerCamera;
    private AudioListener audioListener;
    private Camera disabledMainCamera;
    
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

    public override void OnStartAuthority() {
        base.OnStartAuthority();
        SetupCamera();
        FetchBoundsFromContext();
    }

    public override void OnStartClient() {
        base.OnStartClient();
    }

    private void FetchBoundsFromContext() {
        if (MgOceanContext3D.Instance != null) {
            Bounds bounds = MgOceanContext3D.Instance.GetPlayAreaBounds();
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minZ = bounds.min.z;
            maxZ = bounds.max.z;
            boundsInitialized = true;
        } else {
            Debug.LogError("[MgOceanPlayerController3D] MgOceanContext3D.Instance is null!");
        }
    }

    private void SetupCamera() {
        var cameraObj = new GameObject("TempPlayerCamera_LocalPlayer");
        playerCamera = cameraObj.AddComponent<Camera>();
        
        //required for HDRP to render
        cameraObj.AddComponent<HDAdditionalCameraData>();
        
        audioListener = cameraObj.AddComponent<AudioListener>();
        playerCamera.tag = "MainCamera";
        playerCamera.nearClipPlane = 0.1f;
        playerCamera.farClipPlane = 1000f;

        // Position camera behind the player using world offset initially
        Vector3 targetPosition = transform.position + cameraOffset;
        cameraObj.transform.position = targetPosition;
        cameraObj.transform.LookAt(transform.position);
        
        Debug.Log($"[MgOceanPlayerController3D] Camera created at {targetPosition}, looking at {transform.position}, offset: {cameraOffset}");
    }

    void LateUpdate() {
        if (!isOwned || playerCamera == null) {
            return;
        }

        // Calculate camera position: behind the boat based on its rotation
        Vector3 desiredOffset = transform.rotation * cameraOffset;
        Vector3 targetPosition = transform.position + desiredOffset;
        
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
        if (!boundsInitialized) {
            return;
        }

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
        if (isOwned && playerCamera != null) {
            Destroy(playerCamera.gameObject);
        }
    }
}