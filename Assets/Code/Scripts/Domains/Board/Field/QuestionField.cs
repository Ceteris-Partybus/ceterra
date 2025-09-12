using UnityEngine;
using UnityEngine.Splines;

public class QuestionField : Field {
    public QuestionField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position, float normalizedSplinePosition)
        : base(id, splineId, FieldType.QUESTION, splineKnotIndex, position, normalizedSplinePosition) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
    }
}