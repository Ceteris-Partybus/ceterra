using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MgOcean3DLocalPlayerHUD : NetworkedSingleton<MgOcean3DLocalPlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private UIDocument countdownDocument;

    private Label scoreLabel;
    private Label countdownLabel;

    protected override void Start() {
        base.Start();
        
        if (uiDocument != null) {
            var root = uiDocument.rootVisualElement;
            scoreLabel = root.Q<Label>("local-player-score");
        }

        if (countdownDocument != null) {
            var countdownRoot = countdownDocument.rootVisualElement;
            countdownLabel = countdownRoot.Q<Label>("countdown-label");
        }
    }

    public void UpdateScore(int score) {
        if (scoreLabel == null) {
            return;
        }

        scoreLabel.text = $"{score}";
    }

    public void UpdateCountdown(float timeLeft) {
        if (countdownLabel == null) {
            return;
        }

        var t = Mathf.Max(0f, timeLeft);
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        countdownLabel.text = $"{minutes:00}:{seconds:00}";
    }
}