using UnityEngine;
using UnityEngine.Splines;

public class QuestionField : Field {
    public QuestionField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.QUESTION, splineKnotIndex, position) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player {player.playerName} landed on a question field.");
    }
}