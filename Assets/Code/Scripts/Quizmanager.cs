using UnityEngine;
using System.Collections.Generic;
using System;

public class QuizManager : MonoBehaviour {

    [SerializeField]
    private string questionsFileName;
    private List<QuestionData> allQuestions;

    void Start() {
        LoadQuestions();
        if (allQuestions != null && allQuestions.Count > 0) {
            ShowQuestion(getRandomQuestion());
        }
        else {
            Debug.LogError("Keine Fragen geladen!");
        }
    }

    private QuestionData getRandomQuestion() {
        if (allQuestions == null || allQuestions.Count == 0) {
            Debug.LogError("Keine Fragen verfügbar!");
            return null;
        }

        var randomIndex = UnityEngine.Random.Range(0, allQuestions.Count);
        allQuestions.RemoveAt(randomIndex);
        return allQuestions[randomIndex];
    }

    private void LoadQuestions() {
        allQuestions = JsonReader.LoadJsonArrayFromResources<QuestionData>(questionsFileName);

        if (allQuestions != null) {
            Debug.Log($"Erfolgreich {allQuestions.Count} Fragen geladen.");
        }
    }

    private void ShowQuestion(QuestionData currentQuestion) {
        Debug.Log("Frage: " + currentQuestion.question);
        for (var i = 0; i < currentQuestion.answerOptions.Count; i++) {
            Debug.Log($"Option {i + 1}: {currentQuestion.answerOptions[i]}");
        }
        Debug.Log($"Schwierigkeit: {currentQuestion.difficulty}");
    }

    public bool CheckAnswer(QuestionData currentQuestion, int selectedOptionIndex) {
        return currentQuestion.CheckCorrectAnswer(selectedOptionIndex);
    }
}