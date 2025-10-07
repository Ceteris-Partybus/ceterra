using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using kcp2k;

public class MgMemoryController : NetworkedSingleton<MgMemoryController> {
    [SerializeField] private UIDocument uiDocument;

    private const string LOCALIZATION_TABLE = "ceterra";
    private const long LOCALIZATION_KEY_REWARD = 50359450810896384; // "+{0} Münzen" (DE) / "+{0} Coins" (EN)

    private Label countdownLabel;
    private VisualElement memoryScreen;

    private VisualElement scoreboardScreen;
    private readonly List<Label> playerNameLabels = new();
    private readonly List<Label> playerScoreLabels = new();
    private readonly List<Label> playerRewardLabels = new();
    private readonly List<VisualElement> playerRankElements = new();

    protected override void Start() {
        base.Start();

        var root = uiDocument.rootVisualElement;

        countdownLabel = root.Q<Label>("countdown-label");
        memoryScreen = root.Q<VisualElement>("memory-screen");
        scoreboardScreen = root.Q<VisualElement>("scoreboard-screen");

        for (var i = 1; i <= 4; i++) {
            // Verwende die angepassten Namen aus dem UXML
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