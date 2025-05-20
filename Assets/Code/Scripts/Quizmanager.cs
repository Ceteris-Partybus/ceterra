using UnityEngine;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour {
    public string questionsFileName = "umwelt_fragen";
    private List<QuestionData> allQuestions;
    private QuestionData currentQuestion;

    void Start() {
        LoadQuestions();
        if (allQuestions != null && allQuestions.Count > 0) {
            ShowQuestion(allQuestions[0]);
        }
        else {
            Debug.LogError("Keine Fragen geladen!");
        }
    }

    void LoadQuestions() {
        allQuestions = JsonReader.LoadJsonArrayFromResources<QuestionData>(questionsFileName);

        if (allQuestions != null) {
            Debug.Log($"Erfolgreich {allQuestions.Count} Fragen geladen.");
        }
    }

    void ShowQuestion(QuestionData q_data) {
        currentQuestion = q_data;
        Debug.Log("Frage: " + currentQuestion.question);
        for (int i = 0; i < currentQuestion.answerOptions.Count; i++) {
            Debug.Log($"Option {i + 1}: {currentQuestion.answerOptions[i]}");
        }
        Debug.Log($"Schwierigkeit: {currentQuestion.difficulty}");
        if (!string.IsNullOrEmpty(currentQuestion.category)) {
            Debug.Log($"Kategorie: {currentQuestion.category}");
        }
    }

    public bool CheckAnswer(int selectedOptionIndex) {
        if (currentQuestion == null) {
            return false;
        }

        bool isCorrect = (selectedOptionIndex == currentQuestion.correctAnswerIndex);
        if (isCorrect) {
            Debug.Log("Richtige Antwort!");
        }
        else {
            Debug.Log($"Falsche Antwort! Richtig wäre: {currentQuestion.answerOptions[currentQuestion.correctAnswerIndex]}");
        }
        return isCorrect;
    }

    private void ShuffleList<T>(List<T> list) {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}