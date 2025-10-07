using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MgGarbageLocalPlayerHUD : NetworkedSingleton<MgGarbageLocalPlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private UIDocument countdownDocument;

    private Label scoreLabel;
    private Label countdownLabel;

    protected override void Start() {
        StartCoroutine(WaitForAllPlayers());
        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count());
        }

        base.Start();
        var root = uiDocument.rootVisualElement;
        scoreLabel = root.Q<Label>("local-player-score");

        var countdownRoot = countdownDocument.rootVisualElement;
        countdownLabel = countdownRoot.Q<Label>("countdown-label");
    }

    public void UpdateScore(int score) {
        scoreLabel.text = $"{score}";
    }

    public void UpdateCountdown(float timeLeft) {
        var t = Mathf.Max(0f, timeLeft);
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        countdownLabel.text = $"{minutes:00}:{seconds:00}";
    }
}