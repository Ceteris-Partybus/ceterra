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

    public override long GetEndDescriptionId() => 63999993661964288;
    public override long GetDisplayNameId() => 56648926065164288;
    public override bool IsGlobal() => true;
    protected override int GetCurrentRoundHealthDamage() => DAMAGE_HEALTH[remainingRounds];
    protected override int GetCurrentRoundEnvironmentDamage() => DAMAGE_ENVIRONMENT[remainingRounds];
    protected override int GetCurrentRoundDamageResources() => 0;
    protected override int GetCurrentRoundDamageEconomy() => 0;
    protected override long GetCurrentRoundModalDescriptionId() => MODAL_INFO_TRANSLATION_IDS[remainingRounds];

    protected override IEnumerator Start() {
        skyboxManager.SpawnSmoke(10f);
        yield break;
    }

    protected override IEnumerator Rage() {
        skyboxManager.AddSmokeAttenuation(10f);
        yield break;
    }

    protected override IEnumerator End() {
        skyboxManager.ClearSmoke();
        yield break;
    }
}