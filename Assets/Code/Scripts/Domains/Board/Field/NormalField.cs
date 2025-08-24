using UnityEngine;
using UnityEngine.Splines;

public class NormalField : Field {
    public NormalField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position, Transform transform)
        : base(id, splineId, FieldType.NORMAL, splineKnotIndex, position, transform) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Playerlanded on a normal field.");
    }
}