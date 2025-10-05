using UnityEngine;
using UnityEngine.UIElements;

public class MgOceanLocalPlayerHUD : NetworkedSingleton<MgOceanLocalPlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private UIDocument countdownDocument;

    private Label scoreLabel;
    private Label countdownLabel;

    private void OnEnable() {
        var root = uiDocument.rootVisualElement;
        scoreLabel = root.Q<Label>("local-player-score");
        Debug.Log($"[MgOceanLocalPlayerHUD] OnEnable: scoreLabel found? {scoreLabel != null}");

        var countdownRoot = countdownDocument.rootVisualElement;
        countdownLabel = countdownRoot.Q<Label>("countdown-label");
        Debug.Log($"[MgOceanLocalPlayerHUD] OnEnable: countdownLabel found? {countdownLabel != null}");
    }

    public void UpdateScore(int score) {
        Debug.Log($"[MgOceanLocalPlayerHUD] UpdateScore called with: {score}");
        if (scoreLabel != null) {
            scoreLabel.text = $"{score}";
        }
    }

    public void UpdateCountdown(float timeLeft) {
        if (countdownLabel != null) {
            var t = Mathf.Max(0f, timeLeft);
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            countdownLabel.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
