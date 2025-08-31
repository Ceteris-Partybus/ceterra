using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;

public class BoardquizController : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private BoardquizService boardquizService;

    private const float AUTO_ADVANCE_DELAY = 1.5f;
    private const int MAX_QUESTIONS = 3;
    private const float RESULTS_SCREEN_DURATION = 5f;
    private const float PROGRESS_DOT_BORDER_ACTIVE_WIDTH = 3f;

    private static readonly StyleColor COLOR_CORRECT = new StyleColor(new Color(31f / 255f, 156f / 255f, 51f / 255f));
    private static readonly StyleColor COLOR_INCORRECT = new StyleColor(new Color(183f / 255f, 22f / 255f, 58f / 255f));
    private static readonly StyleColor COLOR_DEFAULT_PROGRESS = new StyleColor(new Color(164f / 255f, 154f / 255f, 154f / 255f));
    private static readonly StyleColor COLOR_BORDER_PROGRESS_ACTIVE = new StyleColor(Color.white);
    private static readonly StyleColor COLOR_BUTTON_DEFAULT_BACKGROUND = new StyleColor(new Color(77f / 255f, 152f / 255f, 157f / 255f));

    private VisualElement root;
    private VisualElement quizArea;
    private VisualElement resultsScreen;
    private Button playAgainButton;
    private Label questionLabel;
    private Label resultsLabel;
    private readonly List<Button> answerButtons = new List<Button>();
    private readonly List<VisualElement> progressDots = new List<VisualElement>();

    private QuestionData currentDisplayedQuestion;
    private Coroutine autoAdvanceCoroutine;
    private Coroutine autoCloseCoroutine;
    private int questionsAnswered = 0;
    private int correctAnswers = 0;
    private BoardPlayer currentPlayer;
    private readonly List<Action> answerButtonActions = new List<Action>();

    public void InitializeQuizForPlayer(BoardPlayer player) {
        this.currentPlayer = player;
    }

    void OnEnable() {
        root = uiDocument.rootVisualElement;
        InitializeUIElements();
        StartQuiz();
    }

    void OnDisable() {
        UnregisterButtonCallbacks();
        StopAutoAdvanceTimer();
        if (autoCloseCoroutine != null) {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private void InitializeUIElements() {
        quizArea = root.Q<VisualElement>("quizArea");
        resultsScreen = root.Q<VisualElement>("resultsScreen");
        questionLabel = quizArea.Q<Label>("questionLabel");
        resultsLabel = resultsScreen.Q<Label>("resultsLabel");
        playAgainButton = resultsScreen.Q<Button>("playAgainButton");
        SetElementDisplay(playAgainButton, false);

        var answerButtonsContainer = quizArea.Q<VisualElement>("answerButtonsContainer");
        answerButtons.Clear();
        for (var i = 0; i < 3; i++) {
            answerButtons.Add(answerButtonsContainer.Q<Button>($"answerButton{i}"));
        }

        var progressIndicator = root.Q<VisualElement>("Progressindicator");
        progressDots.Clear();
        for (var i = 0; i < MAX_QUESTIONS; i++) {
            progressDots.Add(progressIndicator.Q<VisualElement>($"progressDot{i}"));
        }
        SetElementDisplay(quizArea, false);
        SetElementDisplay(resultsScreen, false);
    }

    private void RegisterButtonCallbacks() {
        answerButtonActions.Clear();
        for (var i = 0; i < answerButtons.Count; i++) {
            var currentButtonIndex = i;
            Action action = () => OnAnswerSelected(currentButtonIndex);
            answerButtons[i].clicked += action;
            answerButtonActions.Add(action);
        }
    }

    private void UnregisterButtonCallbacks() {
        for (var i = 0; i < answerButtons.Count; i++) {
            if (i < answerButtonActions.Count && answerButtons[i] != null && answerButtonActions[i] != null) {
                answerButtons[i].clicked -= answerButtonActions[i];
            }
        }
        answerButtonActions.Clear();
    }

    public void StartQuiz() {
        RegisterButtonCallbacks();
        SetElementDisplay(quizArea, true);
        SetElementDisplay(resultsScreen, false);
        questionsAnswered = 0;
        correctAnswers = 0;
        ResetProgressDots();
        LoadAndDisplayNextQuestion();
    }

    private void CloseQuizUI() {
        AwardReward();
        gameObject.SetActive(false);
    }

    private void LoadAndDisplayNextQuestion() {
        StopAutoAdvanceTimer();
        if (questionsAnswered >= MAX_QUESTIONS) {
            EndQuiz();
            return;
        }
        UpdateProgressDotBorder();
        currentDisplayedQuestion = boardquizService.GetRandomQuestion();
        if (currentDisplayedQuestion != null) {
            DisplayQuestion(currentDisplayedQuestion);
        }
    }

    private void EndQuiz() {
        SetElementDisplay(quizArea, false);
        UpdateResultsScreen();
        SetElementDisplay(resultsScreen, true);
        autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
    }

    private void UpdateResultsScreen() {
        var resultText = $"Du hast {correctAnswers} von {MAX_QUESTIONS} Fragen richtig beantwortet!";
        if (correctAnswers > 0) {
            var totalReward = boardquizService.CalculateTotalReward(correctAnswers);
            resultText += $"\nDu erhältst {totalReward} Coins!";
        }
        resultsLabel.text = resultText;
    }

    private void OnAnswerSelected(int selectedAnswerIndex) {
        SetAnswerButtonsState(false);
        StopAutoAdvanceTimer();
        var isCorrect = boardquizService.CheckAnswer(currentDisplayedQuestion, selectedAnswerIndex);
        if (isCorrect) {
            correctAnswers++;
        }
        var correctIndex = currentDisplayedQuestion.correctAnswerIndex;
        if (questionsAnswered < progressDots.Count) {
            progressDots[questionsAnswered].style.backgroundColor = isCorrect ? COLOR_CORRECT : COLOR_INCORRECT;
        }
        questionsAnswered++;
        for (var i = 0; i < answerButtons.Count; i++) {
            if (i != selectedAnswerIndex && i != correctIndex) {
                answerButtons[i].style.opacity = 0.5f;
            }
        }
        if (isCorrect) {
            answerButtons[selectedAnswerIndex].style.backgroundColor = COLOR_CORRECT;
        }
        else {
            answerButtons[selectedAnswerIndex].style.backgroundColor = COLOR_INCORRECT;
            if (correctIndex >= 0 && correctIndex < answerButtons.Count) {
                answerButtons[correctIndex].style.backgroundColor = COLOR_CORRECT;
            }
        }
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
    }

    private void AwardReward() {
        if (currentPlayer != null && correctAnswers > 0) {
            var totalReward = boardquizService.CalculateTotalReward(correctAnswers);
            Debug.Log($"Spieler {currentPlayer.PlayerName} erhält {totalReward} Coins für {correctAnswers} richtige Antworten.");
            currentPlayer.CmdClaimQuizReward(totalReward);
        }
    }

    private IEnumerator AutoAdvanceAfterDelay() {
        yield return new WaitForSeconds(AUTO_ADVANCE_DELAY);
        autoAdvanceCoroutine = null;
        LoadAndDisplayNextQuestion();
    }

    private IEnumerator AutoCloseAfterDelay() {
        yield return new WaitForSeconds(RESULTS_SCREEN_DURATION);
        CloseQuizUI();
    }

    private void StopAutoAdvanceTimer() {
        if (autoAdvanceCoroutine != null) {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
    }

    private void SetElementDisplay(VisualElement element, bool visible) {
        if (element != null) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void ResetProgressDots() {
        foreach (var dot in progressDots) {
            dot.style.backgroundColor = COLOR_DEFAULT_PROGRESS;
            SetDotBorder(dot, 0, default);
        }
    }

    private void UpdateProgressDotBorder() {
        for (var i = 0; i < progressDots.Count; i++) {
            SetDotBorder(progressDots[i], i == questionsAnswered ? PROGRESS_DOT_BORDER_ACTIVE_WIDTH : 0, COLOR_BORDER_PROGRESS_ACTIVE);
        }
    }

    private void SetDotBorder(VisualElement dot, float width, StyleColor color) {
        dot.style.borderLeftWidth = dot.style.borderRightWidth = dot.style.borderTopWidth = dot.style.borderBottomWidth = width;
        if (width > 0) {
            dot.style.borderLeftColor = dot.style.borderRightColor = dot.style.borderTopColor = dot.style.borderBottomColor = color;
        }
    }

    private void DisplayQuestion(QuestionData qData) {
        questionLabel.text = qData.question;
        ResetButtonAppearance();
        SetAnswerButtonsState(true);
        for (var i = 0; i < answerButtons.Count; i++) {
            if (i < qData.answerOptions.Count) {
                answerButtons[i].text = qData.answerOptions[i];
                SetElementDisplay(answerButtons[i], true);
            }
            else {
                SetElementDisplay(answerButtons[i], false);
            }
        }
    }

    private void ResetButtonAppearance() {
        foreach (var button in answerButtons) {
            button.style.backgroundColor = COLOR_BUTTON_DEFAULT_BACKGROUND;
            button.style.opacity = 1.0f;
        }
    }

    private void SetAnswerButtonsState(bool enabled) {
        foreach (var btn in answerButtons) {
            btn.SetEnabled(enabled);
        }
    }
}