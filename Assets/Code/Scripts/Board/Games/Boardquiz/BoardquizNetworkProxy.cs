using Mirror;
using UnityEngine;

public class BoardquizNetworkProxy : NetworkBehaviour {
    public BoardquizController boardquizController;

    [TargetRpc]
    public void TargetShowQuiz(NetworkConnection target) {
        boardquizController.gameObject.SetActive(true);
    }
}