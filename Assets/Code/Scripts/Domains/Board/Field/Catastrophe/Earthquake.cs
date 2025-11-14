using System.Collections;

public class Earthquake : CatastropheEffect {
    private const int ROUNDS = 1;
    private const int DAMAGE_ENVIRONMENT = 25;
    private const int DAMAGE_HEALTH = 15;
    private const int EFFECT_RADIUS = 30;

    public Earthquake() : base(ROUNDS) { }

    protected override IEnumerator Rage() {
        if (base.remainingRounds == ROUNDS) {
            yield break;
        }
    }

    protected override IEnumerator Start() {
        throw new System.NotImplementedException();
    }

    public override long GetEndDescriptionId() {
        throw new System.NotImplementedException();
    }

    public override long GetDisplayNameId() {
        throw new System.NotImplementedException();
    }

    public override bool IsGlobal() {
        throw new System.NotImplementedException();
    }

    protected override int GetCurrentRoundHealthDamage() {
        throw new System.NotImplementedException();
    }

    protected override int GetCurrentRoundEnvironmentDamage() {
        throw new System.NotImplementedException();
    }

    protected override int GetCurrentRoundDamageResources() {
        throw new System.NotImplementedException();
    }

    protected override long GetCurrentRoundModalDescriptionId() {
        throw new System.NotImplementedException();
    }

    protected override int GetCurrentRoundDamageEconomy() {
        throw new System.NotImplementedException();
    }
}