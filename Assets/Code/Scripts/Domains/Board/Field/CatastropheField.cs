using UnityEngine;
using UnityEngine.Splines;

public class CatastropheField : Field {
    private bool hasBeenInvoked = false;
    private CatastropheType type;
    private static int catastropheFieldCount = 0;
    private static readonly CatastropheType[] catastropheTypes = (CatastropheType[])System.Enum.GetValues(typeof(CatastropheType));
    private int environmentEffect;
    private int healthEffect;

    public CatastropheField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.CATASTROPHE, splineKnotIndex, position) {
        this.type = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
        this.environmentEffect = CatastropheTypeExtensions.GetEffects(this.type).Item1;
        this.healthEffect = CatastropheTypeExtensions.GetEffects(this.type).Item2;
    }

    public override void Invoke(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player landed on a catastrophe field of type {type}.");
            hasBeenInvoked = true;
            player.healthDisplay.SubtractCurrentValue(healthEffect);
        }
    }
}