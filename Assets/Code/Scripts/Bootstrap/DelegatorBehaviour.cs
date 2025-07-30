using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DelegatorBehaviour : MonoBehaviour {
    void Start() {
        if (!Application.isBatchMode) {
            SceneManager.LoadScene("MainMenu");
        }
    }
}