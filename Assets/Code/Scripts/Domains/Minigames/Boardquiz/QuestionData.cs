using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData {
    public long question;
    public List<long> answerOptions;
    public int correctAnswerIndex;
    public string difficulty;

    public QuestionData() { }

    public QuestionData(long question, List<long> answerOptions, int correctAnswerIndex, string difficulty) {
        this.question = question;
        this.answerOptions = answerOptions;
        this.correctAnswerIndex = correctAnswerIndex;
        this.difficulty = difficulty;
    }

    public bool isCorrectAnswer(int selectedOptionIndex) {
        return selectedOptionIndex == correctAnswerIndex;
    }
}