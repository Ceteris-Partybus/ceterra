using Mirror;
using System.Collections;
using UnityEngine;

public class NormalFieldBehaviour : FieldBehaviour {
    private const int HEALTH_EFFECT = 5;
    private const int MONEY_EFFECT = 5;

    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player landed on a normal field.");
        yield return AddCoinsAndHealth(player);
    }

    [Server]
    private IEnumerator AddCoinsAndHealth(BoardPlayer player) {
        yield return player.TriggerBlockingAnimation(AnimationType.COIN_GAIN, MONEY_EFFECT);
        player.PlayerStats.ModifyCoins(MONEY_EFFECT);

        yield return player.TriggerBlockingAnimation(AnimationType.HEALTH_GAIN, HEALTH_EFFECT);
        player.PlayerStats.ModifyHealth(HEALTH_EFFECT);
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
