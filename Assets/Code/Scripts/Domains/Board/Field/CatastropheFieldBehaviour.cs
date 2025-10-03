using Mirror;
using UnityEngine;

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

    [Server]
    protected override void OnFieldInvoked(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player landed on a catastrophe field of type {catastropheType}.");
            hasBeenInvoked = true;
            var positionXCurrentPlayer = player.SplineKnotIndex.Spline;
            var positionYCurrentPlayer = player.SplineKnotIndex.Knot;
            foreach (var currentPlayer in FindObjectsByType<BoardPlayer>(FindObjectsSortMode.None)) {
                var positionXIterationPlayer = currentPlayer.SplineKnotIndex.Spline;
                var positionYIterationPlayer = currentPlayer.SplineKnotIndex.Knot;
                if (positionXCurrentPlayer - effectRadius <= positionXIterationPlayer && positionXCurrentPlayer + effectRadius >= positionXIterationPlayer) {
                    if (positionYCurrentPlayer - effectRadius <= positionYIterationPlayer && positionYCurrentPlayer + effectRadius >= positionYIterationPlayer) {
                        currentPlayer.RemoveHealth((uint)healthEffect);
                        Debug.Log($"Player {currentPlayer} got health removed by amount {healthEffect}");
                    }
                }
            }
            BoardContext.Instance.UpdateEnvironmentStat((uint)environmentEffect);
        }
        else {
            player.IsAnimationFinished = true;
        }
        CompleteFieldInvocation();
    }
}
