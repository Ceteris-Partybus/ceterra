using System.Collections;

public class Earthquake : CatastropheEffect {
    private const int ROUNDS = 1;
    private const int DAMAGE_ENVIRONMENT = 25;
    private const int DAMAGE_HEALTH = 15;
    private const int EFFECT_RADIUS = 30;

    public Earthquake() : base(ROUNDS) { }

    protected override IEnumerator OnRaging() {
        if (base.remainingRounds == ROUNDS) {
            yield break;
        }
    }

    public override IEnumerator OnCatastropheRages() {
        throw new System.NotImplementedException();
    }

    public override IEnumerator OnCatastropheEnds() {
        throw new System.NotImplementedException();
    }
}