using Mirror;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class MgOceanPlayerController : NetworkBehaviour {

    void Update() {
        // Wir müssen hier auf "isOwned" prüfen und nicht auf "authority" oder "isLocalPlayer"
        if (!isOwned) {
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, moveY, 0f) * Time.deltaTime * 5f;
        transform.position += move;
    }

    void Start() {
        ChangeAvatarColor(Color.green);
        
    }
    private void ChangeAvatarColor(Color color) {
        if (!isOwned) {
            return;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = color;
        }
        }

}