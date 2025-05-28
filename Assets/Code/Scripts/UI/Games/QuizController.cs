using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System;

public class QuizController : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private QuizService quizService;

    private const float AUTO_ADVANCE_DELAY = 1.5f;
    private const int MAX_QUESTIONS = 3;
    private const float PROGRESS_DOT_BORDER_ACTIVE_WIDTH = 3f;

    private static readonly StyleColor COLOR_CORRECT = new StyleColor(new Color(31f / 255f, 156f / 255f, 51f / 255f));
    private static readonly StyleColor COLOR_INCORRECT = new StyleColor(new Color(183f / 255f, 22f / 255f, 58f / 255f));
    private static readonly StyleColor COLOR_DEFAULT_PROGRESS = new StyleColor(new Color(164f / 255f, 154f / 255f, 154f / 255f));
    private static readonly StyleColor COLOR_BORDER_PROGRESS_ACTIVE = new StyleColor(Color.white);
    private static readonly StyleColor COLOR_BUTTON_DEFAULT_BACKGROUND = new StyleColor(new Color(77f / 255f, 152f / 255f, 157f / 255f));

    private VisualElement root;
    private VisualElement quizArea;
    private VisualElement resultsScreen;
    private Button playButton;
    private Button playAgainButton;
    private Label questionLabel;
    private Label resultsLabel;
    private List<Button> answerButtons = new List<Button>();
    private List<VisualElement> progressDots = new List<VisualElement>();

    private QuestionData currentDisplayedQuestion;
    private Coroutine autoAdvanceCoroutine;
    private int questionsAnswered = 0;
    private int correctAnswers = 0;

    private List<Action> answerButtonActions = new List<Action>();

    void OnEnable() {
        root = uiDocument.rootVisualElement;

        InitializeUIElements();
        RegisterButtonCallbacks();
    }

    void OnDisable() {
        UnregisterButtonCallbacks();
        StopAutoAdvanceTimer();
    }

    private void InitializeUIElements() {
        playButton = root.Q<Button>("playButton");
        quizArea = root.Q<VisualElement>("quizArea");
        resultsScreen = root.Q<VisualElement>("resultsScreen");

        questionLabel = quizArea.Q<Label>("questionLabel");
        resultsLabel = resultsScreen.Q<Label>("resultsLabel");
        playAgainButton = resultsScreen.Q<Button>("playAgainButton");

        var answerButtonsContainer = quizArea.Q<VisualElement>("answerButtonsContainer");

        answerButtons.Clear();
        for (var answerButtonIndex = 0; answerButtonIndex < 3; answerButtonIndex++) {
            var btn = answerButtonsContainer.Q<Button>($"answerButton{answerButtonIndex}");
            answerButtons.Add(btn);
        }

        var progressIndicator = root.Q<VisualElement>("Progressindicator");

        progressDots.Clear();
        for (var progressDotIndex = 0; progressDotIndex < MAX_QUESTIONS; progressDotIndex++) {
            var dot = progressIndicator.Q<VisualElement>($"progressDot{progressDotIndex}");
            progressDots.Add(dot);
        }

        SetElementDisplay(playButton, true);
        SetElementDisplay(quizArea, false);
        SetElementDisplay(resultsScreen, false);
    }

    private void SetElementDisplay(VisualElement element, bool visible) {
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ResetProgressDots() {
        foreach (var dot in progressDots) {
            dot.style.backgroundColor = COLOR_DEFAULT_PROGRESS;
            SetDotBorder(dot, 0, default);
        }
    }

    private void UpdateProgressDotBorder() {
        for (var progressDotIndex = 0; progressDotIndex < progressDots.Count; progressDotIndex++) {
            if (progressDotIndex == questionsAnswered) {
                SetDotBorder(progressDots[progressDotIndex], PROGRESS_DOT_BORDER_ACTIVE_WIDTH, COLOR_BORDER_PROGRESS_ACTIVE);
            }
            else {
                SetDotBorder(progressDots[progressDotIndex], 0, default);
            }
        }
    }

    private void SetDotBorder(VisualElement dot, float width, StyleColor color) {
        dot.style.borderLeftWidth = width;
        dot.style.borderRightWidth = width;
        dot.style.borderTopWidth = width;
        dot.style.borderBottomWidth = width;

        if (width > 0) {
            dot.style.borderLeftColor = color;
            dot.style.borderRightColor = color;
            dot.style.borderTopColor = color;
            dot.style.borderBottomColor = color;
        }
    }

    private void RegisterButtonCallbacks() {
        playButton.clicked += StartQuiz;
        playAgainButton.clicked += StartQuizAgain;

        answerButtonActions.Clear();

        for (var answerButtonIndex = 0; answerButtonIndex < answerButtons.Count; answerButtonIndex++) {
            var currentButtonIndex = answerButtonIndex;
            Action action = () => OnAnswerSelected(currentButtonIndex);
            answerButtons[answerButtonIndex].clicked += action;
            answerButtonActions.Add(action);
        }
    }

    private void UnregisterButtonCallbacks() {
        playButton.clicked -= StartQuiz;
    }

    private void StartQuiz() {
        SetElementDisplay(playButton, false);
        SetElementDisplay(quizArea, true);

        questionsAnswered = 0;
        correctAnswers = 0;
        ResetProgressDots();
        LoadAndDisplayNextQuestion();
    }

    private void StartQuizAgain() {
        SetElementDisplay(resultsScreen, false);
        StartQuiz();
    }

    private void LoadAndDisplayNextQuestion() {
        StopAutoAdvanceTimer();

        if (questionsAnswered >= MAX_QUESTIONS) {
            EndQuiz();
            return;
        }

        UpdateProgressDotBorder();

        currentDisplayedQuestion = quizService.GetRandomQuestion();

        if (currentDisplayedQuestion != null) {
            DisplayQuestion(currentDisplayedQuestion);
        }
    }

    private void EndQuiz() {
        SetElementDisplay(quizArea, false);
        UpdateResultsScreen();
        SetElementDisplay(resultsScreen, true);
    }

    private void UpdateResultsScreen() {
        var resultText = $"Du hast {correctAnswers} von {MAX_QUESTIONS} Fragen richtig beantwortet!";

        resultsLabel.text = resultText;
    }

    private void DisplayQuestion(QuestionData qData) {
        questionLabel.text = qData.question;
        ResetButtonAppearance();
        SetAnswerButtonsState(true);

        for (var answerButtonIndex = 0; answerButtonIndex < answerButtons.Count; answerButtonIndex++) {
            if (answerButtonIndex < qData.answerOptions.Count) {
                answerButtons[answerButtonIndex].text = qData.answerOptions[answerButtonIndex];
                SetElementDisplay(answerButtons[answerButtonIndex], true);
            }
            else {
                SetElementDisplay(answerButtons[answerButtonIndex], false);
            }
        }
    }

    private void OnAnswerSelected(int selectedAnswerIndex) {
        SetAnswerButtonsState(false);
        StopAutoAdvanceTimer();

        var isCorrect = quizService.CheckAnswer(currentDisplayedQuestion, selectedAnswerIndex);
        if (isCorrect) {
            correctAnswers++;
        }

        var correctIndex = currentDisplayedQuestion.correctAnswerIndex;

        if (questionsAnswered < progressDots.Count) {
            progressDots[questionsAnswered].style.backgroundColor = isCorrect ? COLOR_CORRECT : COLOR_INCORRECT;
        }

        questionsAnswered++;

        for (var answerButtonIndex = 0; answerButtonIndex < answerButtons.Count; answerButtonIndex++) {
            if (answerButtonIndex != selectedAnswerIndex && answerButtonIndex != correctIndex) {
                answerButtons[answerButtonIndex].style.opacity = 0.5f;
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

    private IEnumerator AutoAdvanceAfterDelay() {
        yield return new WaitForSeconds(AUTO_ADVANCE_DELAY);
        autoAdvanceCoroutine = null;
        LoadAndDisplayNextQuestion();
    }

    private void StopAutoAdvanceTimer() {
        if (autoAdvanceCoroutine != null) {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
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