using Mirror;
using UnityEngine;

public class MgOceanTrash3D : NetworkBehaviour {
    [Header("Score Settings")]
    [SerializeField] private int points = 10;

    public int Points => points;

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
