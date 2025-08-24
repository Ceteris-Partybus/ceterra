using UnityEngine;
using UnityEngine.Splines;

public class EventField : Field {
    public EventField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position, Transform transform)
        : base(id, splineId, FieldType.NORMAL, splineKnotIndex, position, transform) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");
    }
}