using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

public class MgQuizduelController : NetworkedSingleton<MgQuizduelController> {
    [SerializeField] private UIDocument uiDocument;

    private const string LOCALIZATION_TABLE = "ceterra";
    private const long LOCALIZATION_KEY_REWARD = 50359450810896384; // "+{0} Münzen" (DE) / "+{0} Coins" (EN)
    private const long LOCALIZATION_KEY_WAITING = 50355143541706752; // "Warten auf andere Spieler..." (DE) / "Waiting for other players..." (EN)

    private static readonly StyleColor COLOR_CORRECT = new StyleColor(new Color(31f / 255f, 156f / 255f, 51f / 255f));
    private static readonly StyleColor COLOR_INCORRECT = new StyleColor(new Color(183f / 255f, 22f / 255f, 58f / 255f));
    private static readonly StyleColor COLOR_DEFAULT_PROGRESS = new StyleColor(new Color(164f / 255f, 154f / 255f, 154f / 255f));
    private static readonly StyleColor COLOR_BORDER_PROGRESS_ACTIVE = new StyleColor(Color.white);

    private Label countdownLabel;
    private Label questionLabel;
    private Button answerButton0;
    private Button answerButton1;
    private Button answerButton2;
    private readonly List<VisualElement> progressDots = new();
    private Label waitingLabel;

    private VisualElement quizScreen;
    private VisualElement scoreboardScreen;
    private VisualElement waitingScreen;
    private readonly List<Label> playerNameLabels = new();
    private readonly List<Label> playerScoreLabels = new();
    private readonly List<Label> playerRewardLabels = new();
    private readonly List<VisualElement> playerRankElements = new();

    private int dotCount = 0;
    private IVisualElementScheduledItem waitingAnimation;

    private int currentQuestionIndex = 0;
    public int CurrentQuestionIndex => currentQuestionIndex;

    protected override void Start() {
        base.Start();

        var root = uiDocument.rootVisualElement;

        countdownLabel = root.Q<Label>("countdown-label");
        questionLabel = root.Q<Label>("questionLabel");

        answerButton0 = root.Q<Button>("answerButton0");
        answerButton1 = root.Q<Button>("answerButton1");
        answerButton2 = root.Q<Button>("answerButton2");

        var progressIndicator = root.Q<VisualElement>("Progressindicator");
        progressDots.Clear();
        for (var i = 0; i < MgQuizduelContext.Instance.MaxQuestions; i++) {
            progressDots.Add(progressIndicator.Q<VisualElement>($"progressDot{i}"));
        }

        quizScreen = root.Q<VisualElement>("quiz-screen");
        scoreboardScreen = root.Q<VisualElement>("scoreboard-screen");
        waitingScreen = root.Q<VisualElement>("waiting-screen");
        waitingLabel = root.Q<Label>("waiting-label");

        for (var i = 1; i <= 4; i++) {
            playerNameLabels.Add(root.Q<Label>($"player-name-{i}"));
            playerScoreLabels.Add(root.Q<Label>($"player-score-{i}"));

            var rewardContainer = root.Q<VisualElement>($"player-reward-{i}");
            var rewardLabel = rewardContainer?.Q<Label>();
            playerRewardLabels.Add(rewardLabel);

            playerRankElements.Add(root.Q<VisualElement>($"player-rank-{i}"));
        }

        InitializeProgressDots();
        SetupAnswerButtonEvents();
    }

    private void SetupAnswerButtonEvents() {
        answerButton0.clicked += () => OnAnswerButtonClicked(0);
        answerButton1.clicked += () => OnAnswerButtonClicked(1);
        answerButton2.clicked += () => OnAnswerButtonClicked(2);
    }

    private void OnAnswerButtonClicked(int answerIndex) {
        UpdateQuestionResult(currentQuestionIndex, MgQuizduelContext.Instance.ProcessPlayerAnswer(answerIndex));
        MoveToNextQuestion();
    }

    public void UpdateQuestionResult(int questionIndex, bool isCorrect) {
        if (questionIndex < progressDots.Count) {
            progressDots[questionIndex].style.backgroundColor = isCorrect ? COLOR_CORRECT : COLOR_INCORRECT;
            SetDotBorder(progressDots[questionIndex], 0, default);
        }
    }

    public void MoveToNextQuestion() {
        currentQuestionIndex++;

        if (currentQuestionIndex >= MgQuizduelContext.Instance.MaxQuestions) {
            DisableAnswerButtons();
            ShowWaitingMessage();
        }
        else {
            UpdateProgressDotBorder();
            SetQuizUI(MgQuizduelContext.Instance.GetQuestion(currentQuestionIndex));
        }
    }

    public void UpdateCountdown(float timeLeft) {
        var minutes = Mathf.FloorToInt(timeLeft / 60f);
        var seconds = Mathf.FloorToInt(timeLeft % 60f);
        countdownLabel.text = $"{minutes:00}:{seconds:00}";
    }

    private void InitializeProgressDots() {
        foreach (var dot in progressDots) {
            dot.style.backgroundColor = COLOR_DEFAULT_PROGRESS;
            SetDotBorder(dot, 0, default);
        }

        currentQuestionIndex = 0;
        UpdateProgressDotBorder();
    }

    private void UpdateProgressDotBorder() {
        for (var i = 0; i < progressDots.Count; i++) {
            SetDotBorder(progressDots[i], 0, default);
        }

        if (currentQuestionIndex < progressDots.Count) {
            SetDotBorder(progressDots[currentQuestionIndex], 3f, COLOR_BORDER_PROGRESS_ACTIVE);
        }
    }

    private void SetDotBorder(VisualElement dot, float width, StyleColor color) {
        dot.style.borderLeftWidth = dot.style.borderRightWidth = dot.style.borderTopWidth = dot.style.borderBottomWidth = width;
        if (width > 0) {
            dot.style.borderLeftColor = dot.style.borderRightColor = dot.style.borderTopColor = dot.style.borderBottomColor = color;
        }
    }

    [Server]
    public void ShowScoreboard(List<MgQuizduelPlayerRankingData> playerRankings) {
        RpcShowScoreboard(playerRankings);
    }

    [ClientRpc]
    private void RpcShowScoreboard(List<MgQuizduelPlayerRankingData> playerRankings) {
        SetElementDisplay(quizScreen, false);
        SetElementDisplay(scoreboardScreen, true);

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
        }
    }

    private string GetLocalizedRewardText(int reward) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, LOCALIZATION_KEY_REWARD, new object[] { reward })
        .Result;
    }

    [Server]
    public void UpdateQuizUI(QuestionData questionData) {
        RpcUpdateQuizUI(questionData);
    }

    [ClientRpc]
    private void RpcUpdateQuizUI(QuestionData questionData) {
        HideWaitingMessage();
        SetQuizUI(questionData);
    }

    private void SetQuizUI(QuestionData questionData) {
        questionLabel.text = LocalizationManager.Instance.GetLocalizedText(questionData.question);
        answerButton0.text = LocalizationManager.Instance.GetLocalizedText(questionData.answerOptions[0]);
        answerButton1.text = LocalizationManager.Instance.GetLocalizedText(questionData.answerOptions[1]);
        answerButton2.text = LocalizationManager.Instance.GetLocalizedText(questionData.answerOptions[2]);
    }

    private void SetElementDisplay(VisualElement element, bool visible) {
        if (element != null) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void DisableAnswerButtons() {
        answerButton0.SetEnabled(false);
        answerButton1.SetEnabled(false);
        answerButton2.SetEnabled(false);

        answerButton0.style.opacity = 0.5f;
        answerButton1.style.opacity = 0.5f;
        answerButton2.style.opacity = 0.5f;
    }
    private void ShowWaitingMessage() {
        SetElementDisplay(waitingScreen, true);
        StartWaitingAnimation();
    }

    private void HideWaitingMessage() {
        StopWaitingAnimation();
        SetElementDisplay(waitingScreen, false);
    }

    private void StartWaitingAnimation() {
        dotCount = 0;
        waitingAnimation = waitingLabel.schedule.Execute(AnimateWaitingText).Every(500);
    }

    private void StopWaitingAnimation() {
        waitingAnimation?.Pause();
        waitingAnimation = null;
    }

    private void AnimateWaitingText() {
        dotCount = (dotCount + 1) % 4;
        var dots = new string('.', dotCount);

        // Get the base text from localization (without dots)
        var baseText = LocalizationSettings.StringDatabase
            .GetLocalizedStringAsync(LOCALIZATION_TABLE, LOCALIZATION_KEY_WAITING)
            .Result;

        waitingLabel.text = $"{baseText}{dots}";
    }
}