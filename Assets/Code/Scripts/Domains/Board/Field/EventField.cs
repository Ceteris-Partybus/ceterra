using UnityEngine;
using UnityEngine.Splines;

public class EventField : Field {
    public EventField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.EVENT, splineKnotIndex, position) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player landed on an event field.");
    }
}