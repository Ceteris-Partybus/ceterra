using Mirror;
using UnityEngine;

public class MgOceanTrash3D : NetworkBehaviour {
    [Header("Score Settings")]
    [SerializeField] private int points = 10;

    [Header("Floating Animation")]
    [SerializeField] private float bobAmplitude = 0.6f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationAmplitude = 10f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Wave Drift")]
    [SerializeField] private float waveAmplitudeX = 1.2f;
    [SerializeField] private float waveSpeedX = 0.4f;
    [SerializeField] private float waveAmplitudeZ = 1.5f;
    [SerializeField] private float waveSpeedZ = 0.5f;

    public int Points => points;

    private Vector3 startPosition;
    private float timeOffset;

    private void Start() {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update() {
        float time = Time.time + timeOffset;
        
        Vector3 pos = startPosition;
        pos.x += Mathf.Sin(time * waveSpeedX) * waveAmplitudeX;
        pos.z += Mathf.Sin(time * waveSpeedZ) * waveAmplitudeZ;
        pos.y += Mathf.Sin(time * bobSpeed) * bobAmplitude;
        transform.position = pos;

        // Gentle rotation wobble
        float rotY = Mathf.Sin(time * rotationSpeed) * rotationAmplitude;
        float rotZ = Mathf.Sin(time * rotationSpeed * 0.8f) * rotationAmplitude;
        transform.rotation = Quaternion.Euler(0f, rotY, rotZ);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"[MgOceanTrash3D] OnTriggerEnter called! Object: {other.gameObject.name}, Tag: {other.tag}, IsServer: {isServer}");

        if (!isServer) return;

        if (!other.CompareTag("Player")) return;

        var playerController = other.GetComponent<MgOceanPlayerController3D>();
        if (playerController == null || playerController.connectionToClient == null) return;

        var oceanPlayer = playerController.connectionToClient.identity.GetComponent<MgOceanPlayer3D>();
        if (oceanPlayer == null) return;

        Debug.Log($"[MgOceanTrash3D] SUCCESS! Adding {points} points");
        oceanPlayer.ServerAddScore(points);
        NetworkServer.Destroy(gameObject);
    }
}
