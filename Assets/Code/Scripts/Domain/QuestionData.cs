using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData {
    public string question;
    public List<string> answerOptions;
    public int correctAnswerIndex;
    public string difficulty;

    public QuestionData() { }

    public QuestionData(string question, List<string> answerOptions, int correctAnswerIndex, string difficulty) {
        this.question = question;
        this.answerOptions = answerOptions;
        this.correctAnswerIndex = correctAnswerIndex;
        this.difficulty = difficulty;
    }

    public bool isCorrectAnswer(int selectedOptionIndex) {
        return selectedOptionIndex == correctAnswerIndex;
    }
}