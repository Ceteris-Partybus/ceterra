using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class QuizService : MonoBehaviour {
    [SerializeField] private string questionsFileName;

    private List<QuestionData> availableQuestions;

    public QuizService(String questionsFileName) {
        this.questionsFileName = questionsFileName;
    }

    void Awake() {
        LoadQuestions();
    }

    private void LoadQuestions() {
        availableQuestions = JsonConvert.DeserializeObject<List<QuestionData>>(questionsFileName);

        if (availableQuestions == null) {
            availableQuestions = new List<QuestionData>();
        }

        if (availableQuestions.Count > 0) {
            Debug.Log($"QuizService: Erfolgreich {availableQuestions.Count} Fragen geladen.");
        }
        else {
            Debug.LogWarning("QuizService: Keine Fragen aus der Datei geladen oder die Datei war leer.");
        }
    }

    public QuestionData GetRandomQuestion() {
        if (availableQuestions.Count == 0) {
            Debug.Log("QuizService: Keine Fragen mehr im Pool verfügbar.");
            return null;
        }

        var randomIndex = UnityEngine.Random.Range(0, availableQuestions.Count);
        QuestionData randomQuestion = availableQuestions[randomIndex];
        availableQuestions.RemoveAt(randomIndex);
        return randomQuestion;
    }

    public bool CheckAnswer(QuestionData currentQuestion, int selectedOptionIndex) {
        return currentQuestion.isCorrectAnswer(selectedOptionIndex);
    }

    public void SetDataSourcePath(string filePath) {
        questionsFileName = filePath;
        LoadQuestions();
    }
}