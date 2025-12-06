using Mirror;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Rigidbody))]
public class MgOceanPlayerController3D : NetworkBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float waterDepth = 0f;

    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplier = 2.5f;
    [SerializeField] private float boostDuration = 5f;
    [SerializeField] private float boostDecayStep = 0.25f;
    [SerializeField] private KeyCode boostKey = KeyCode.Space;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 30f, -55f);
    [SerializeField] private float cameraSmoothSpeed = 5f;

    [Header("Floating Animation")]
    [SerializeField] private float bobAmplitude = 0.4f;
    [SerializeField] private float bobSpeed = 1.8f;
    [SerializeField] private float tiltAmplitude = 3f;
    [SerializeField] private float tiltSpeed = 1.5f;

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

    private float currentBoostMultiplier = 1f;
    private float boostTimer = 0f;
    private bool isBoosting = false;

    private float floatTimeOffset;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        
        // Only freeze rotation to prevent tipping - allow position for physics collisions
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection for fast objects
        
        floatTimeOffset = Random.Range(0f, Mathf.PI * 2f);
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

        // Calculate camera position using only Y rotation (ignore tilt from floating animation)
        Quaternion yawRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Vector3 desiredOffset = yawRotation * cameraOffset;
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

        // Boost activation
        if (Input.GetKeyDown(boostKey) && !isBoosting) {
            ActivateBoost();
        }

        if (isBoosting) {
            UpdateBoostDecay();
        }

        ApplyFloatingAnimation();
    }

    private void ApplyFloatingAnimation() {
        float time = Time.time + floatTimeOffset;
        
        // Gentle bobbing on Y axis
        Vector3 pos = transform.position;
        float bobOffset = Mathf.Sin(time * bobSpeed) * bobAmplitude;
        pos.y = waterDepth + bobOffset;
        transform.position = pos;

        // Gentle tilt/wobble rotation
        float tiltX = Mathf.Sin(time * tiltSpeed) * tiltAmplitude;
        float tiltZ = Mathf.Sin(time * tiltSpeed * 0.5f) * tiltAmplitude;
        
        // Combine base Y rotation with tilt
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(tiltX, currentEuler.y, tiltZ);
    }

    void FixedUpdate() {
        if (!isOwned) {
            return;
        }

        ApplyMovement();
    }

    private void ActivateBoost() {
        isBoosting = true;
        currentBoostMultiplier = boostMultiplier;
        boostTimer = boostDuration;
    }

    private void UpdateBoostDecay() {
        boostTimer -= Time.deltaTime;

        if (boostTimer <= 0f) {
            currentBoostMultiplier -= boostDecayStep;

            if (currentBoostMultiplier <= 1f) {
                currentBoostMultiplier = 1f;
                isBoosting = false;
            } else {
                boostTimer = boostDuration;
            }
        }
    }

    private void ApplyMovement() {
        if (!boundsInitialized) {
            return;
        }

        // Keep boat at water level
        Vector3 pos = rb.position;
        pos.y = waterDepth;
        rb.position = pos;

        if (Mathf.Abs(inputTurn) > 0.01f) {
            float rotationAmount = inputTurn * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAmount, 0f));
        }

        if (Mathf.Abs(inputForward) > 0.01f) {
            float currentSpeed = moveSpeed * currentBoostMultiplier;
            Vector3 movement = transform.forward * inputForward * currentSpeed * Time.fixedDeltaTime;
            Vector3 newPosition = rb.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            newPosition.y = waterDepth;

            rb.MovePosition(newPosition);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Optional: Log boat collisions for debugging
        if (collision.gameObject.CompareTag("Player")) {
            Debug.Log($"[MgOceanPlayerController3D] Boat collision with another boat!");
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"[MgOceanPlayerController3D] BOAT triggered by: {other.gameObject.name}");
    }

    void OnDestroy() {
        if (isOwned && playerCamera != null) {
            Destroy(playerCamera.gameObject);
        }
    }
}