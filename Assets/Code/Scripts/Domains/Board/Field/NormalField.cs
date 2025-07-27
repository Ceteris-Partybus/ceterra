using UnityEngine;
using UnityEngine.Splines;

public class NormalField : Field {
    public NormalField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.NORMAL, splineKnotIndex, position) {
    }

    public override void Invoke(Player player) {
        Debug.Log($"Player {player.playerName} landed on a normal field.");
    }
}