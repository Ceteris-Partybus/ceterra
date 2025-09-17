using Mirror;
using UnityEngine;
using UnityEngine.Splines;

public class NormalFieldBehaviour : FieldBehaviour {
    private const int HEALTHEFFECT = 5;
    private const int MONEYEFFECT = 5;

    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player landed on a normal field.");
        player.AddCoins(MONEYEFFECT);
        player.AddHealth(HEALTHEFFECT);

        CompleteFieldInvocation();
    }
}
