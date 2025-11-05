using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

public class MgMemoryController : NetworkedSingleton<MgMemoryController> {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float factPopupDuration = 10f; // Anzeigedauer des Fact-Popups in Sekunden
    [SerializeField] private Camera canvasCamera; // Referenz zur Canvas-Kamera

    private const string LOCALIZATION_TABLE = "ceterra";
    private const long LOCALIZATION_KEY_REWARD = 50359450810896384; // "+{0} Münzen" (DE) / "+{0} Coins" (EN)

    private float originalSortOrder; // Ursprüngliche Sort Order speichern

    private Label countdownLabel;
    private Label scoreLabel;
    private Label currentPlayerLabel;
    private VisualElement memoryScreen;

    private VisualElement scoreboardScreen;
    private VisualElement factPopup;
    private readonly List<Label> playerNameLabels = new();
    private readonly List<Label> playerScoreLabels = new();
    private readonly List<Label> playerRewardLabels = new();
    private readonly List<VisualElement> playerRankElements = new();

    protected override void Start() {
        base.Start();

        var root = uiDocument.rootVisualElement;

        countdownLabel = root.Q<Label>("countdown-label");
        scoreLabel = root.Q<Label>("score-label");
        currentPlayerLabel = root.Q<Label>("current-player-label");
        memoryScreen = root.Q<VisualElement>("memory-screen");
        scoreboardScreen = root.Q<VisualElement>("scoreboard-screen");
        factPopup = root.Q<VisualElement>("fact-popup");

        // Ursprüngliche Sort Order der Canvas-Kamera speichern
        if (canvasCamera != null) {
            originalSortOrder = canvasCamera.depth;
        }

        for (var i = 1; i <= 4; i++) {
            playerNameLabels.Add(root.Q<Label>($"player-name-{i}"));
            playerScoreLabels.Add(root.Q<Label>($"player-score-{i}"));

            var rewardContainer = root.Q<VisualElement>($"player-reward-{i}");
            var rewardLabel = rewardContainer?.Q<Label>();
            playerRewardLabels.Add(rewardLabel);

            playerRankElements.Add(root.Q<VisualElement>($"player-rank-{i}"));
        }
    }

    public void UpdateCountdown(float timeLeft) {
        var minutes = Mathf.FloorToInt(timeLeft / 60f);
        var seconds = Mathf.FloorToInt(timeLeft % 60f);
        countdownLabel.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdatePlayerScore(int score) {
        scoreLabel.text = score.ToString();
    }

    public void UpdateCurrentPlayer(string playerName) {
        currentPlayerLabel.text = playerName;
    }

    [Server]
    public void ShowFactPopup() {
        RpcShowFactPopup();
    }

    [ClientRpc]
    private void RpcShowFactPopup() {
        // Canvas-Kamera Sort Order auf 0 setzen
        if (canvasCamera != null) {
            canvasCamera.depth = 0;
        }

        SetElementDisplay(factPopup, true);
        StartCoroutine(HideFactPopupAfterDelay(factPopupDuration));
    }

    private System.Collections.IEnumerator HideFactPopupAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        SetElementDisplay(factPopup, false);

        // Canvas-Kamera Sort Order zurücksetzen
        if (canvasCamera != null) {
            canvasCamera.depth = originalSortOrder;
        }
    }

    [Server]
    public void ShowScoreboard(/*List<MgQuizduelPlayerRankingData> playerRankings*/) {
        Debug.Log("Showing scoreboard on clients");
        RpcShowScoreboard(/*playerRankings*/);
    }

    [ClientRpc]
    private void RpcShowScoreboard() {
        Debug.Log("RpcShowScoreboard called on client");

        SetElementDisplay(memoryScreen, false);
        SetElementDisplay(scoreboardScreen, true);

        /*
                for (var i = 0; i < playerRankElements.Count; i++) {
                    if (i < playerRankings.Count) {

                        playerNameLabels[i].text = playerRankings[i].playerName;
                        playerScoreLabels[i].text = $"{playerRankings[i].score}/{MgQuizduelContext.Instance.MaxQuestions}";
                        playerRewardLabels[i].text = GetLocalizedRewardText(playerRankings[i].reward);

                        SetElementDisplay(playerRankElements[i], true);
                    }
                    else {
                        SetElementDisplay(playerRankElements[i], false);
                    }
                } */
    }

    private string GetLocalizedRewardText(int reward) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, LOCALIZATION_KEY_REWARD, new object[] { reward })
        .Result;
    }

    private void SetElementDisplay(VisualElement element, bool visible) {
        if (element != null) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}