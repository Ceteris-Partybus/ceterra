using System.Collections;
using System.Linq;
using UnityEngine;

public class Drought : CatastropheEffect {
    private const int ROUNDS = 5;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 5, 5, 5, 5, 5 };
    private static readonly int[] DAMAGE_RESOURCES = { 5, 7, 10, 7, 5 };
    private static readonly int[] DAMAGE_HEALTH = { 5, 6, 7, 6, 5 };
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 66478602922311683, 66478602922311682, 66478602922311681, 66478602922311680, 66478602888757248 };

    public Drought() : base(ROUNDS) { }

    public override long GetEndDescriptionId() => 66479609035177984;
    public override long GetDisplayNameId() => 56649684747649024;
    public override bool IsGlobal() => true;
    protected override int GetCurrentRoundHealthDamage() => DAMAGE_HEALTH[remainingRounds];
    protected override int GetCurrentRoundEnvironmentDamage() => DAMAGE_ENVIRONMENT[remainingRounds];
    protected override int GetCurrentRoundDamageResources() => DAMAGE_RESOURCES[remainingRounds];
    protected override int GetCurrentRoundDamageEconomy() => 0;
    protected override long GetCurrentRoundModalDescriptionId() => MODAL_INFO_TRANSLATION_IDS[remainingRounds];
}