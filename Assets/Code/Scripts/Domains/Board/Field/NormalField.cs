using UnityEngine;
using UnityEngine.Splines;

public class NormalField : Field {
    public NormalField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.NORMAL, splineKnotIndex, position) {
    }

    private const int HEALTHEFFECT = 5;
    private const int MONEYEFFECT = 5;

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Playerlanded on a normal field.");
        player.moneyDisplay.AddCurrentValue(MONEYEFFECT);
        player.healthDisplay.AddCurrentValue(HEALTHEFFECT);
    }
}