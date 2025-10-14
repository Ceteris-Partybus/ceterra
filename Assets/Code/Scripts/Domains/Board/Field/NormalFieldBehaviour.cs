using Mirror;
using System.Collections;
using UnityEngine;

public class NormalFieldBehaviour : FieldBehaviour {
    private const int HEALTH_EFFECT = 5;
    private const int MONEY_EFFECT = 5;

    protected override void OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player landed on a normal field.");
        StartCoroutine(AddCoinsAndHealth(player));
    }

    [Server]
    private IEnumerator AddCoinsAndHealth(BoardPlayer player) {
        TargetPlayMoneyHealthSound(player.connectionToClient);
        yield return player.TriggerBlockingAnimation(AnimationType.COIN_GAIN, MONEY_EFFECT);
        player.PlayerStats.ModifyCoins(MONEY_EFFECT);

        yield return player.TriggerBlockingAnimation(AnimationType.HEALTH_GAIN, HEALTH_EFFECT);
        player.PlayerStats.ModifyHealth(HEALTH_EFFECT);

        CompleteFieldInvocation();
    }

    [TargetRpc]
    private void TargetPlayMoneyHealthSound(NetworkConnectionToClient target) {
        StartCoroutine(PlaySoundDelayed());
    }

    private IEnumerator PlaySoundDelayed() {
        yield return new WaitForSeconds(0.6f);
        Audiomanager.Instance?.PlayMoneyHealthSound();
    }
}
