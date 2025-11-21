using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using System;

public class MgMemoryController : NetworkedSingleton<MgMemoryController> {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float factPopupDuration = 10f; // Anzeigedauer des Fact-Popups in Sekunden
    [SerializeField] private Canvas canvas;

    private const string LOCALIZATION_TABLE = "ceterra";
    private const long LOCALIZATION_KEY_REWARD = 50359450810896384; // "+{0} Münzen" (DE) / "+{0} Coins" (EN)

    private Label scoreLabel;
    private Label currentPlayerLabel;
    private VisualElement memoryScreen;

    private VisualElement scoreboardScreen;
    private VisualElement factPopup;
    private Label factTitle;
    private Label factDescription;
    private VisualElement factImage;
    private Label progressText;
    private readonly List<Label> playerNameLabels = new();
    private readonly List<Label> playerScoreLabels = new();
    private readonly List<Label> playerRewardLabels = new();
    private readonly List<VisualElement> playerRankElements = new();

    protected override void Start() {
        base.Start();

        var root = uiDocument.rootVisualElement;

        scoreLabel = root.Q<Label>("score-label");
        currentPlayerLabel = root.Q<Label>("current-player-label");
        memoryScreen = root.Q<VisualElement>("memory-screen");
        scoreboardScreen = root.Q<VisualElement>("scoreboard-screen");
        factPopup = root.Q<VisualElement>("fact-popup");
        factTitle = root.Q<Label>("fact-title");
        factDescription = root.Q<Label>("fact-description");
        factImage = root.Q<VisualElement>("fact-image");

        progressText = root.Q<Label>("progress-text");

        for (var i = 1; i <= 4; i++) {
            playerNameLabels.Add(root.Q<Label>($"player-name-{i}"));
            playerScoreLabels.Add(root.Q<Label>($"player-score-{i}"));

            var rewardContainer = root.Q<VisualElement>($"player-reward-{i}");
            var rewardLabel = rewardContainer?.Q<Label>();
            playerRewardLabels.Add(rewardLabel);

            playerRankElements.Add(root.Q<VisualElement>($"player-rank-{i}"));
        }
    }

    public void UpdatePlayerScore(int score) {
        scoreLabel.text = score.ToString();
    }

    public void UpdateCurrentPlayer(string playerName) {
        currentPlayerLabel.text = playerName;
    }

    [Server]
    public void ShowFactPopup(MemoryFactData factData) {
        RpcShowFactPopup(factData);
    }

    [ClientRpc]
    private void RpcShowFactPopup(MemoryFactData factData) {
        canvas.sortingOrder = 0;
        factTitle.text = factData.title;
        this.factDescription.text = factData.description;

        var sprite = Resources.Load<Sprite>(factData.imagePath);
        factImage.style.backgroundImage = new StyleBackground(sprite);

        SetElementDisplay(factPopup, true);

        StartCoroutine(HideFactPopupAfterDelay(factPopupDuration));
        StartCoroutine(UpdateFactPopupCountdown(factPopupDuration));
    }

    private IEnumerator UpdateFactPopupCountdown(float duration) {
        var elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var remainingSeconds = Mathf.CeilToInt(duration - elapsed);
            progressText.text = remainingSeconds.ToString();
            yield return null;
        }

        progressText.text = "0";
    }

    private IEnumerator HideFactPopupAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        SetElementDisplay(factPopup, false);

        canvas.sortingOrder = 1;
    }

    [Server]
    public void ShowScoreboard(List<MgMemoryPlayerRankingData> playerRankings) {
        RpcShowScoreboard(playerRankings);
    }

    [ClientRpc]
    private void RpcShowScoreboard(List<MgMemoryPlayerRankingData> playerRankings) {
        SetElementDisplay(memoryScreen, false);
        SetElementDisplay(scoreboardScreen, true);

        for (var i = 0; i < playerRankElements.Count; i++) {
            if (i < playerRankings.Count) {

                playerNameLabels[i].text = playerRankings[i].playerName;
                playerScoreLabels[i].text = playerRankings[i].score.ToString();
                playerRewardLabels[i].text = GetLocalizedRewardText(playerRankings[i].reward);

                SetElementDisplay(playerRankElements[i], true);
            }
            else {
                SetElementDisplay(playerRankElements[i], false);
            }
        }
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