using System.Collections;
using UnityEngine;

public class Drought : CatastropheEffect {
    private const int ROUNDS = 5;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 5, 5, 5, 5, 5 };
    private static readonly int[] DAMAGE_RESOURCES = { 5, 7, 10, 7, 5 };
    private static readonly int[] DAMAGE_ECONOMY = { 5, 7, 10, 7, 5 };
    private static readonly int[] DAMAGE_HEALTH = { 5, 6, 7, 6, 5 };
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 66478602922311683, 66478602922311682, 66478602922311681, 66478602922311680, 66478602888757248 };

    private const float BASE_SUNLIGHT = 100000f;
    private const float PEAK_SUNLIGHT = 240000f;

    private SkyboxManager skyboxManager;

    public Drought(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    public override long GetEndDescriptionId() => 66479609035177984;
    public override long GetDisplayNameId() => 56649684747649024;
    public override bool IsGlobal() => true;
    protected override int GetCurrentRoundHealthDamage() => DAMAGE_HEALTH[remainingRounds - 1];
    protected override int GetCurrentRoundEnvironmentDamage() => DAMAGE_ENVIRONMENT[remainingRounds - 1];
    protected override int GetCurrentRoundDamageResources() => DAMAGE_RESOURCES[remainingRounds - 1];
    protected override int GetCurrentRoundDamageEconomy() => 0;
    protected override long GetCurrentRoundModalDescriptionId() => MODAL_INFO_TRANSLATION_IDS[remainingRounds - 1];

    private float GetCurrentRoundSunlight() {
        float progress;
        if (remainingRounds >= 2) {
            progress = (ROUNDS - remainingRounds) / (ROUNDS - 2f);
        }
        else {
            progress = remainingRounds / 2f;
        }

        return Mathf.Lerp(BASE_SUNLIGHT, PEAK_SUNLIGHT, progress);
    }

    protected override IEnumerator Start() {
        skyboxManager.IncreaseSunlight(GetCurrentRoundSunlight());
        yield break;
    }

    protected override IEnumerator Rage() {
        skyboxManager.IncreaseSunlight(GetCurrentRoundSunlight());
        yield break;
    }

    protected override IEnumerator End() {
        skyboxManager.ResetSunlight();
        yield break;
    }
}