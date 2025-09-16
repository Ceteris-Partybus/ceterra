using Mirror;
using UnityEngine;
using UnityEngine.Splines;

public class CatastropheFieldBehaviour : FieldBehaviour {
    [SyncVar]
    private bool hasBeenInvoked = false;

    [SyncVar]
    private CatastropheType catastropheType;

    private static int catastropheFieldCount = 0;
    private static readonly CatastropheType[] catastropheTypes = (CatastropheType[])System.Enum.GetValues(typeof(CatastropheType));

    private int environmentEffect;

    private int healthEffect;

    private int effectRadius;

    public void Start() {
        this.catastropheType = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
        var effects = CatastropheTypeExtensions.GetEffects(this.catastropheType);
        this.environmentEffect = effects.Item1;
        this.healthEffect = effects.Item2;
        this.effectRadius = effects.Item3;
    }

    protected override void OnFieldInvoked(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player landed on a catastrophe field of type {catastropheType}.");
            hasBeenInvoked = true;
            int positionXCurrentPlayer = player.SplineKnotIndex.Spline;
            int positionYCurrentPlayer = player.SplineKnotIndex.Knot;
            foreach (BoardPlayer currentPlayer in FindObjectsByType<BoardPlayer>(FindObjectsSortMode.None)) {
                int positionXIterationPlayer = currentPlayer.SplineKnotIndex.Spline;
                int positionYIterationPlayer = currentPlayer.SplineKnotIndex.Knot;
                if (positionXCurrentPlayer - effectRadius <= positionXIterationPlayer && positionXCurrentPlayer + effectRadius >= positionXIterationPlayer) {
                    if (positionYCurrentPlayer - effectRadius <= positionYIterationPlayer && positionYCurrentPlayer + effectRadius >= positionYIterationPlayer) {
                        currentPlayer.RemoveHealth((uint)healthEffect);
                        Debug.Log($"Player {currentPlayer} got health removed by amount {healthEffect}");
                    }
                }
            }
            BoardContext.Instance.UpdateEnvironmentStat((uint)environmentEffect);
        }
        CompleteFieldInvocation();
    }
}
