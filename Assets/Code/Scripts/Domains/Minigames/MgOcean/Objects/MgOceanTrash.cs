using Mirror;
using System.Linq;
using UnityEngine;

// NOTE: Ensure this GameObject's prefab has a NetworkTransform component to sync position.
[RequireComponent(typeof(Rigidbody2D))]
public class MgOceanTrash : NetworkBehaviour {
    [SerializeField]
    private float fallSpeed = 2f;

    [SerializeField]
    private MgOceanTrashType trashType;
    public MgOceanTrashType TrashType => trashType;

    [SerializeField]
    private bool isTrigger = false;

    private Rigidbody2D rb;

    public override void OnStartClient() {
        base.OnStartClient();
        
        int trashLayer = LayerMask.NameToLayer("Trash");
        if (trashLayer != -1) {
            gameObject.layer = trashLayer;
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        if (TryGetComponent<Collider2D>(out var collider)) {
            collider.isTrigger = true;
        }
        
        int trashLayer = LayerMask.NameToLayer("Trash");
        if (trashLayer != -1) {
            gameObject.layer = trashLayer;
        }
    }

    [ServerCallback]
    void FixedUpdate() {
        rb.MovePosition(transform.position + Vector3.down * fallSpeed * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos() {
        if (TryGetComponent<BoxCollider2D>(out var boxCollider)) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            var playerController = collision.GetComponent<MgOceanPlayerController>();
            if (playerController != null && playerController.connectionToClient != null) {
                var oceanPlayer = playerController.connectionToClient.identity.GetComponent<MgOceanPlayer>();
                if (oceanPlayer != null) {
                    oceanPlayer.ServerAddScore(1);
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}