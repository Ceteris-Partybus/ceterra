public class Wildfire : CatastropheEffect {
    private const int ROUNDS = 3;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 25, 20, 10 };
    private static readonly int[] DAMAGE_HEALTH = { 15, 10, 5 };

    private SkyboxManager skyboxManager;

    public Wildfire(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    protected override void OnEffectTriggered() {
        if (base.remainingRounds == ROUNDS) {
            skyboxManager.SpawnSmoke(10f);
            return;
        }
        skyboxManager.AddSmokeAttenuation(10f);
    }
}