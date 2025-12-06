using System;
using System.Collections;

public class Earthquake : CatastropheEffect {
    private const int ROUNDS = 2;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 0, 10 };
    private static readonly int[] DAMAGE_RESOURCES = { 15, 0 };
    private static readonly int[] DAMAGE_HEALTH = { 0, 6 };
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 67262900487143424, 67262900449394688 };

    public Earthquake() : base(ROUNDS) { }

    public override CatastropheType GetCatastropheType() => CatastropheType.EARTHQUAKE;
    public override Action GetSoundEmitter() => () => Audiomanager.Instance?.PlayEarthquakeSound();
    public override long GetEndDescriptionId() => 67262900487143425;
    public override long GetDisplayNameId() => 67264907419664384;
    public override bool IsGlobal() => true;
    protected override int GetCurrentRoundHealthDamage() => DAMAGE_HEALTH[remainingRounds - 1];
    protected override int GetCurrentRoundEnvironmentDamage() => DAMAGE_ENVIRONMENT[remainingRounds - 1];
    protected override int GetCurrentRoundDamageResources() => DAMAGE_RESOURCES[remainingRounds - 1];
    protected override int GetCurrentRoundDamageEconomy() => 0;
    protected override long GetCurrentRoundModalDescriptionId() => MODAL_INFO_TRANSLATION_IDS[remainingRounds - 1];

    protected override IEnumerator Start() {
        CameraHandler.Instance.RpcShakeCamera(6f, 8f);
        yield break;
    }
}