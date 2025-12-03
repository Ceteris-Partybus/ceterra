using UnityEngine;

class FramerateLimiter : MonoBehaviour {
    [SerializeField]
    private int targetFramerate = 165;

    private void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFramerate;
    }
}