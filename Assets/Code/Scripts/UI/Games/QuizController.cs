using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class QuizController : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;

    [SerializeField] private QuizService quizService;

    private VisualElement root;
    private VisualElement quizArea;
    private Button playButton;

    private Label questionLabel;
    private Label feedbackLabel;
    private Button nextQuestionButton;
    private List<Button> answerButtons = new List<Button>();
    private QuestionData currentDisplayedQuestion;

    void OnEnable() {
        root = uiDocument.rootVisualElement;

        InitializeUIElements();
        RegisterButtonCallbacks();
    }

    void OnDisable() {
        UnregisterButtonCallbacks();
    }

    private void InitializeUIElements() {
        playButton = root.Q<Button>("playButton");
        quizArea = root.Q<VisualElement>("quizArea");

        questionLabel = quizArea.Q<Label>("questionLabel");
        feedbackLabel = quizArea.Q<Label>("feedbackLabel");
        nextQuestionButton = quizArea.Q<Button>("nextQuestionButton");

        answerButtons.Clear();
        var answerButtonsContainer = quizArea.Q<VisualElement>("answerButtonsContainer");
        if (answerButtonsContainer != null) {
            for (var i = 0; i < 4; i++) { // Assuming max 4 answer buttons as per UXML
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
        if (nextQuestionButton != null) {
            nextQuestionButton.clicked += OnNextQuestionClicked;
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
        if (nextQuestionButton != null) {
            nextQuestionButton.clicked -= OnNextQuestionClicked;
        }
        // It's good practice to also unregister answer button callbacks if they could change
        // For this specific case, they are re-registered in OnEnable if needed,
        // but explicit unregistration is safer if elements are dynamically added/removed.
        foreach (var btn in answerButtons) {
            // To fully unregister, you'd need to store the delegates or use ClearCallbacks() if appropriate.
            // For simplicity here, if answerButtons are always the same instances, this is less critical.
        }
    }

    private void OnPlayButtonClicked() {
        if (playButton != null) {
            playButton.style.display = DisplayStyle.None;
        }

        if (quizArea != null) {
            quizArea.style.display = DisplayStyle.Flex; // Show the quiz area
        }

        LoadAndDisplayNextQuestion();
    }

    private void LoadAndDisplayNextQuestion() {
        currentDisplayedQuestion = quizService.GetRandomQuestion();
        Debug.Log($"Aktuelle Frage: {currentDisplayedQuestion?.question}");

        if (currentDisplayedQuestion != null) {
            DisplayQuestion(currentDisplayedQuestion);
        }
    }

    private void DisplayQuestion(QuestionData qData) {
        questionLabel.text = qData.question;

        feedbackLabel.text = "";
        feedbackLabel.style.color = StyleKeyword.Null;
        nextQuestionButton.style.display = DisplayStyle.None;
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

        bool isCorrect = quizService.CheckAnswer(currentDisplayedQuestion, selectedAnswerIndex);

        if (isCorrect) {
            feedbackLabel.text = "Richtig!";
            feedbackLabel.style.color = Color.green;
        }
        else {
            string correctAnswerText = "Unbekannt";
            if (currentDisplayedQuestion.correctAnswerIndex >= 0 && currentDisplayedQuestion.correctAnswerIndex < currentDisplayedQuestion.answerOptions.Count) {
                correctAnswerText = currentDisplayedQuestion.answerOptions[currentDisplayedQuestion.correctAnswerIndex];
            }
            feedbackLabel.text = $"Falsch. Richtig wäre: {correctAnswerText}";
            feedbackLabel.style.color = Color.red;
        }
        nextQuestionButton.text = "Nächste Frage";
        nextQuestionButton.style.display = DisplayStyle.Flex;
    }

    private void OnNextQuestionClicked() {
        LoadAndDisplayNextQuestion();
    }

    private void SetAnswerButtonsState(bool enabled) {
        foreach (var btn in answerButtons) {
            if (btn != null) {
                btn.SetEnabled(enabled);
            }
        }
    }
}