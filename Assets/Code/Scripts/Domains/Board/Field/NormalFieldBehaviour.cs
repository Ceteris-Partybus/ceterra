using Mirror;
using System.Collections;
using UnityEngine;

public class NormalFieldBehaviour : FieldBehaviour {
    private const int HEALTH_EFFECT = 5;
    private const int MONEY_EFFECT = 5;

    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player landed on a normal field.");
        StartCoroutine(AddCoinsAndHealth(player));
    }

    [Server]
    private IEnumerator AddCoinsAndHealth(BoardPlayer player) {
        player.IsAnimationFinished = false;
        player.AddCoins(MONEY_EFFECT);
        yield return new WaitUntil(() => player.IsAnimationFinished);

        player.IsAnimationFinished = false;
        player.AddHealth(HEALTH_EFFECT);
        yield return new WaitUntil(() => player.IsAnimationFinished);

        CompleteFieldInvocation();
    }
}
