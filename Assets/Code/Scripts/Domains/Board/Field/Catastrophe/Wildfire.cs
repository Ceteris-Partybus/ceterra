using System.Collections;
using System.Linq;
using UnityEngine;

public class Wildfire : CatastropheEffect {
    private const int ROUNDS = 3;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 10, 20, 25 };
    private static readonly int[] DAMAGE_HEALTH = { 5, 10, 15 };
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 65747542009790464, 63999993620021248, 56648972298977280 };

    private SkyboxManager skyboxManager;

    public Wildfire(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    protected override IEnumerator Start() {
        skyboxManager.SpawnSmoke(10f);
        yield return ApplyDamage();
    }

    protected override IEnumerator Rage() {
        skyboxManager.AddSmokeAttenuation(10f);
        yield return ApplyDamage();
    }

    public override IEnumerator End() {
        skyboxManager.ClearSmoke();
        RpcShowCatastropheInfo(null, 63999993661964288, CatastropheType.WILDFIRE);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
    }

    private IEnumerator ApplyDamage() {
        var affectedPlayers = GetAffectedPlayersGlobal(DAMAGE_HEALTH[remainingRounds]);
        RpcShowCatastropheInfo(affectedPlayers.Select(p => p.ToString()).Aggregate((a, b) => a + "\n" + b), MODAL_INFO_TRANSLATION_IDS[remainingRounds], CatastropheType.WILDFIRE);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);

        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnCurrentPlayer();

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(1f);

        BoardContext.Instance.UpdateEnvironmentStat(-1 * DAMAGE_ENVIRONMENT[remainingRounds]);
    }
}