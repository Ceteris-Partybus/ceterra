using Mirror;
using System.Linq;
using UnityEngine;

// NOTE: Ensure this GameObject's prefab has a NetworkTransform component to sync position.
[RequireComponent(typeof(Rigidbody2D))]
public class MgOceanTrash : NetworkBehaviour {
    [SerializeField]
    private float fallSpeed = 2f;

    [SerializeField]
    private float waveAmplitude = 1f;

    [SerializeField]
    private float waveFrequency = 1f;

    [SerializeField]
    private MgOceanTrashType trashType;
    public MgOceanTrashType TrashType => trashType;

    [SyncVar(hook = nameof(OnFlippedChanged))]
    public bool isFlipped;

    private Rigidbody2D rb;
    private float startYPosition;
    private float elapsedTime;
    private Vector2 movementDirection = Vector2.left;

    public override void OnStartClient() {
        base.OnStartClient();
        // Set layer to enable physics interactions (Edit > Project Settings > Physics 2D > Layer Collision Matrix)
        int trashLayer = LayerMask.NameToLayer("Trash");
        if (trashLayer != -1) {
            gameObject.layer = trashLayer;
        }
        OnFlippedChanged(false, isFlipped);
    }

    public override void OnStartServer() {
        base.OnStartServer();
        
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (TryGetComponent<Collider2D>(out var collider)) {
            collider.isTrigger = true;
        }
        
        int trashLayer = LayerMask.NameToLayer("Trash");
        if (trashLayer != -1) {
            gameObject.layer = trashLayer;
        }

        startYPosition = transform.position.y;
        elapsedTime = 0f;
    }

    private void OnFlippedChanged(bool oldFlipped, bool newFlipped) {
        var srs = GetComponentsInChildren<SpriteRenderer>();
        var target = srs.FirstOrDefault(sr => sr.sprite != null && sr.sprite.name == "tortoise");
        if (target != null) {
            target.flipX = newFlipped;
        }
    }

    public void SetMovementDirection(Vector2 direction) {
        movementDirection = direction.normalized;
    }

    [ServerCallback]
    void FixedUpdate() {
        elapsedTime += Time.fixedDeltaTime;
        
        float yOffset = Mathf.Sin(elapsedTime * waveFrequency) * waveAmplitude;
        float newY = startYPosition + yOffset;
        float newX = transform.position.x + movementDirection.x * fallSpeed * Time.fixedDeltaTime;
        
        rb.MovePosition(new Vector3(newX, newY, transform.position.z));
    }

    private void OnDrawGizmos() {
        if (TryGetComponent<BoxCollider2D>(out var boxCollider)) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log($"[MgOceanTrash] OnTriggerEnter2D called with: {collision.gameObject.name}, tag: {collision.tag}, layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        
        if (collision.CompareTag("Player")) {
            Debug.Log($"[MgOceanTrash] Player tag detected!");
            var playerController = collision.GetComponent<MgOceanPlayerController>();
            if (playerController != null && playerController.connectionToClient != null) {
                Debug.Log($"[MgOceanTrash] PlayerController found with connection");
                var oceanPlayer = playerController.connectionToClient.identity.GetComponent<MgOceanPlayer>();
                if (oceanPlayer != null) {
                    Debug.Log($"[MgOceanTrash] Adding score to player {oceanPlayer.PlayerId}");
                    if (trashType == MgOceanTrashType.STANDARD) {
                        oceanPlayer.ServerAddScore(1);
                    } else if (trashType == MgOceanTrashType.DANGEROUS) {
                        oceanPlayer.ServerAddScore(2);
                    } else if (trashType == MgOceanTrashType.ORGANIC) {
                        oceanPlayer.ServerReduceScore(3);
                    }
                    NetworkServer.Destroy(gameObject);
                } 
            } 
        } else {
            Debug.Log($"[MgOceanTrash] Collision object doesn't have Player tag");
        }
    }
}