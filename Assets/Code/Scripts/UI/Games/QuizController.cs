using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class QuizController : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private QuizService quizService;
    [SerializeField] private const float AUTO_ADVANCE_DELAY = 5f;

    private VisualElement root;
    private VisualElement quizArea;
    private Button playButton;

    private Label questionLabel;
    private List<Button> answerButtons = new List<Button>();
    private QuestionData currentDisplayedQuestion;
    private Coroutine autoAdvanceCoroutine;

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

        questionLabel = quizArea.Q<Label>("questionLabel");

        answerButtons.Clear();
        var answerButtonsContainer = quizArea.Q<VisualElement>("answerButtonsContainer");
        if (answerButtonsContainer != null) {
            for (var i = 0; i < 3; i++) {
                var btn = answerButtonsContainer.Q<Button>($"answerButton{i}");
                if (btn != null) {
                    answerButtons.Add(btn);
                }
            }
        }

        if (playButton != null) {
            playButton.style.display = DisplayStyle.Flex;
        }

        if (quizArea != null) {
            quizArea.style.display = DisplayStyle.None;
        }
    }

    private void RegisterButtonCallbacks() {
        if (playButton != null) {
            playButton.clicked += OnPlayButtonClicked;
        }

        for (int i = 0; i < answerButtons.Count; i++) {
            var buttonIndex = i;
            answerButtons[i].clicked += () => OnAnswerSelected(buttonIndex);
        }
    }

    private void UnregisterButtonCallbacks() {
        if (playButton != null) {
            playButton.clicked -= OnPlayButtonClicked;
        }

        for (int i = 0; i < answerButtons.Count; i++) {
            var buttonIndex = i;
            if (answerButtons[i] != null) {
                answerButtons[i].clicked -= () => OnAnswerSelected(buttonIndex);
            }
        }
    }

    private void OnPlayButtonClicked() {
        if (playButton != null) {
            playButton.style.display = DisplayStyle.None;
        }

        if (quizArea != null) {
            quizArea.style.display = DisplayStyle.Flex;
        }

        LoadAndDisplayNextQuestion();
    }

    private void LoadAndDisplayNextQuestion() {
        StopAutoAdvanceTimer();

        currentDisplayedQuestion = quizService.GetRandomQuestion();
        Debug.Log($"Aktuelle Frage: {currentDisplayedQuestion?.question}");

        if (currentDisplayedQuestion != null) {
            DisplayQuestion(currentDisplayedQuestion);
        }
    }

    private void DisplayQuestion(QuestionData qData) {
        questionLabel.text = qData.question;
        ResetButtonAppearance();
        SetAnswerButtonsState(true);

        for (int i = 0; i < answerButtons.Count; i++) {
            if (i < qData.answerOptions.Count) {
                answerButtons[i].text = qData.answerOptions[i];
                answerButtons[i].style.display = DisplayStyle.Flex;
            }
            else {
                answerButtons[i].style.display = DisplayStyle.None;
            }
        }
    }

    private void OnAnswerSelected(int selectedAnswerIndex) {
        SetAnswerButtonsState(false);
        StopAutoAdvanceTimer();

        bool isCorrect = quizService.CheckAnswer(currentDisplayedQuestion, selectedAnswerIndex);
        int correctIndex = currentDisplayedQuestion.correctAnswerIndex;

        for (int i = 0; i < answerButtons.Count; i++) {
            if (i != selectedAnswerIndex && i != correctIndex) {
                answerButtons[i].style.opacity = 0.5f;
            }
        }

        if (isCorrect) {
            answerButtons[selectedAnswerIndex].style.backgroundColor = new StyleColor(new Color(0.2f, 0.8f, 0.2f));
        }
        else {
            answerButtons[selectedAnswerIndex].style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f));

            if (correctIndex >= 0 && correctIndex < answerButtons.Count) {
                answerButtons[correctIndex].style.backgroundColor = new StyleColor(new Color(0.2f, 0.8f, 0.2f));
            }
        }

        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
    }

    private IEnumerator AutoAdvanceAfterDelay() {
        yield return new WaitForSeconds(AUTO_ADVANCE_DELAY);
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
            button.style.backgroundColor = new StyleColor(new Color(77 / 255f, 152 / 255f, 157 / 255f));
            button.style.opacity = 1.0f;
        }
    }

    private void SetAnswerButtonsState(bool enabled) {
        foreach (var btn in answerButtons) {
            if (btn != null) {
                btn.SetEnabled(enabled);
            }
        }
    }
}