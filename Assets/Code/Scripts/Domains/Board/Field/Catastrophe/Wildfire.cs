using System.Collections;
using System.Linq;
using UnityEngine;

public class Wildfire : CatastropheEffect {
    private const int ROUNDS = 3;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 25, 20, 10 };
    private static readonly int[] DAMAGE_HEALTH = { 15, 10, 5 };
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 56648972298977280, 63999993620021248, 63999993661964288 };

    private SkyboxManager skyboxManager;

    public Wildfire(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    public override IEnumerator OnCatastropheRages() {
        skyboxManager.SpawnSmoke(10f);
        yield return ApplyDamage();
    }

    protected override IEnumerator OnRaging() {
        skyboxManager.AddSmokeAttenuation(10f);
        yield return ApplyDamage();
    }

    public override IEnumerator OnCatastropheEnds() {
        skyboxManager.ClearSmoke();
        yield return null;
    }

    private IEnumerator ApplyDamage() {
        var affectedPlayers = GetAffectedPlayersGlobal(DAMAGE_HEALTH[remainingRounds % ROUNDS]);
        RpcShowCatastropheInfo(affectedPlayers.Select(p => p.ToString()).Aggregate((a, b) => a + "\n" + b), MODAL_INFO_TRANSLATION_IDS[remainingRounds % ROUNDS], CatastropheType.WILDFIRE);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);

        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnCurrentPlayer();

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(1f);

        BoardContext.Instance.UpdateEnvironmentStat(-1 * DAMAGE_ENVIRONMENT[remainingRounds % ROUNDS]);
    }
}