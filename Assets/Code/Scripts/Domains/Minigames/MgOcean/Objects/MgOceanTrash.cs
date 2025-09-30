using Mirror;
using UnityEngine;

public class MgOceanTrash : NetworkBehaviour {
    [SerializeField]
    private float fallSpeed = 2f;

    [SerializeField]
    private MgOceanTrashType trashType;
    public MgOceanTrashType TrashType => trashType;

    [ClientCallback]
    void Update() {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    public override void OnStartClient() {
        base.OnStartClient();
    }

    [ClientCallback]
    void OnCollisionEnter2D(Collision2D collision) {
        HandleCollision(collision.gameObject);
    }

    [ClientCallback]
    void OnTriggerEnter2D(Collider2D collision) {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject collisionObject) {
        if (collisionObject.CompareTag("Player")) {
            Debug.Log($"Trash {gameObject.name} collided with player {collisionObject.name}");
            var player = collisionObject.GetComponent<MgOceanPlayer>();
            if (player != null) {
                Debug.Log($"Adding score to player {player.PlayerId}");
                player.AddScore(1);
                
                if (isServer) {
                    NetworkServer.Destroy(gameObject);
                } else {
                    CmdDestroyTrash();
                }
            }
        }
    }
    
    [Command(requiresAuthority = false)]
    private void CmdDestroyTrash() {
        NetworkServer.Destroy(gameObject);
    }
}