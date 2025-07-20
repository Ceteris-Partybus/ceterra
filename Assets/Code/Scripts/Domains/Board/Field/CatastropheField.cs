using UnityEngine;
using UnityEngine.Splines;

public class CatastropheField : Field {
    private bool hasBeenInvoked = false;
    private CatastropheType type;
    private static int catastropheFieldCount = 0;
    private static readonly CatastropheType[] catastropheTypes = (CatastropheType[])System.Enum.GetValues(typeof(CatastropheType));

    public CatastropheField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.CATASTROPHE, splineKnotIndex, position) {
        this.type = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
    }

    public override void Invoke(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player {player.playerName} landed on a catastrophe field of type {type}.");
            hasBeenInvoked = true;
        }
    }
}