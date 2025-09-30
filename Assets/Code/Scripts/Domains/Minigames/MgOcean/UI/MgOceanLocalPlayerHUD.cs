using UnityEngine;
using UnityEngine.UIElements;

public class MgOceanLocalPlayerHUD : NetworkedSingleton<MgOceanLocalPlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private UIDocument countdownDocument;

    private Label scoreLabel;
    private Label countdownLabel;

    protected override void Start() {
        base.Start();
        var root = uiDocument.rootVisualElement;
        scoreLabel = root.Q<Label>("local-player-score");

        var countdownRoot = countdownDocument.rootVisualElement;
        countdownLabel = countdownRoot.Q<Label>("countdown-label");
    }

    public void UpdateScore(uint score) {
        scoreLabel.text = $"{score}";
    }

    public void UpdateCountdown(float timeLeft) {
        var t = Mathf.Max(0f, timeLeft);
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        countdownLabel.text = $"{minutes:00}:{seconds:00}";
    }
}
