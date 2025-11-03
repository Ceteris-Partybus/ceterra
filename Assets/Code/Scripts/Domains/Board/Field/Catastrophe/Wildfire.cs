public class Wildfire : CatastropheEffect {
    private static int ROUNDS = 3;
    private SkyboxManager skyboxManager;

    public Wildfire(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    protected override void OnEffectTriggered() {
        if (base.remainingRounds == ROUNDS) {
            skyboxManager.SpawnSmoke(20f);
            return;
        }
        skyboxManager.AddSmokeAttenuation(20f);
    }
}