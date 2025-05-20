using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData {
    public string question;
    public List<string> answerOptions;
    public int correctAnswerIndex;
    public string difficulty;

    public QuestionData() { }

    public bool CheckCorrectAnswer(int selectedOptionIndex) {
        return selectedOptionIndex == correctAnswerIndex;
    }

}