using Mirror;
using System.Collections;

public class NormalFieldBehaviour : FieldBehaviour {
    private const int HEALTH_EFFECT = 5;
    private const int MONEY_EFFECT = 5;

    public override FieldType GetFieldType() => FieldType.NORMAL;

    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        yield return AddCoinsAndHealth(player);
    }

    [Server]
    private IEnumerator AddCoinsAndHealth(BoardPlayer player) {
        yield return player.TriggerBlockingAnimation(AnimationType.COIN_GAIN, MONEY_EFFECT);
        player.PlayerStats.ModifyCoins(MONEY_EFFECT);

        yield return player.TriggerBlockingAnimation(AnimationType.HEALTH_GAIN, HEALTH_EFFECT);
        player.PlayerStats.ModifyHealth(HEALTH_EFFECT);
    }
}
