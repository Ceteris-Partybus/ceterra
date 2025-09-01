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

    public override void OnStartServer() {
        base.OnStartServer();
        this.catastropheType = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
        var effects = CatastropheTypeExtensions.GetEffects(this.catastropheType);
        this.environmentEffect = effects.Item1;
        this.healthEffect = effects.Item2;
    }

    protected override void OnFieldInvoked(BoardPlayer player) {
        if (!hasBeenInvoked) {
            Debug.Log($"Player landed on a catastrophe field of type {catastropheType}.");
            hasBeenInvoked = true;
            player.RemoveHealth((uint)healthEffect);
        }

        CompleteFieldInvocation();
    }
}
