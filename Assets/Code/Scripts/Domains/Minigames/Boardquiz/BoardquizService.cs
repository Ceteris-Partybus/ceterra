using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BoardquizService : MonoBehaviour {
    [SerializeField] private string questionsFileName;
    private List<QuestionData> availableQuestions;
    [SerializeField]
    private uint COINS_PER_CORRECT_ANSWER = 10;

    void Awake() {
        LoadQuestions();
    }

    private void LoadQuestions() {
        var jsonFile = Resources.Load<TextAsset>(questionsFileName);

        if (jsonFile == null) {
            Debug.LogError($"QuizService: Datei '{questionsFileName}' nicht gefunden.");
            availableQuestions = new List<QuestionData>();
            return;
        }

        try {
            availableQuestions = JsonConvert.DeserializeObject<List<QuestionData>>(jsonFile.text);
        }
        catch (System.Exception ex) {
            Debug.LogError($"QuizService: Fehler beim Laden der JSON-Datei: {ex.Message}");
            availableQuestions = new List<QuestionData>();
        }

        if (availableQuestions != null && availableQuestions.Count > 0) {
            Debug.Log($"QuizService: Erfolgreich {availableQuestions.Count} Fragen geladen.");
        }
        else {
            Debug.LogWarning("QuizService: Keine Fragen aus der Datei geladen oder die Datei war leer.");
        }
    }

    public QuestionData GetRandomQuestion() {
        if (availableQuestions == null) {
            LoadQuestions();
        }

        if (availableQuestions.Count == 0) {
            return null;
        }

        var randomIndex = Random.Range(0, availableQuestions.Count);
        var randomQuestion = availableQuestions[randomIndex];
        availableQuestions.RemoveAt(randomIndex);
        return randomQuestion;
    }

    public bool CheckAnswer(QuestionData currentQuestion, int selectedOptionIndex) {
        return currentQuestion.isCorrectAnswer(selectedOptionIndex);
    }

    public uint CalculateTotalReward(int numberOfCorrectAnswers) {
        return (uint)numberOfCorrectAnswers * COINS_PER_CORRECT_ANSWER;
    }

    public void SetDataSourcePath(string filePath) {
        questionsFileName = filePath;
        LoadQuestions();
    }
}
