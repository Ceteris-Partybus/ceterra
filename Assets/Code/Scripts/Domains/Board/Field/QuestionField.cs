using UnityEngine;
using UnityEngine.Splines;

public class QuestionField : Field {
    public QuestionField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.QUESTION, splineKnotIndex, position) {
    }

    private QuestionData currentQuestion;
    private const int HEALTHEFFECT = 10;
    private const int MONEYEFFECT = 10;

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
        currentQuestion = BoardContext.quizService.GetRandomQuestion();
    }

    public void CheckAnswer(BoardPlayer player, int selectedOptionIndex) {
        if (BoardContext.quizService.CheckAnswer(currentQuestion, selectedOptionIndex)) {
            player.AddCoins(MONEYEFFECT);
            player.AddHealth(HEALTHEFFECT);
        }
    }
}