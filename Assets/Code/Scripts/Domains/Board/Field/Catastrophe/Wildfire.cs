public class Wildfire : CatastropheEffect {
    private const int ROUNDS = 3;
    private static readonly int[] DAMAGE_ENVIRONMENT = { 25, 20, 10 };
    private static readonly int[] DAMAGE_HEALTH = { 15, 10, 5 };

    private SkyboxManager skyboxManager;

    public Wildfire(SkyboxManager skyboxManager) : base(ROUNDS) {
        this.skyboxManager = skyboxManager;
    }

    public override void OnCatastropheRages() {
        skyboxManager.SpawnSmoke(10f);
    }

    protected override void OnRaging() {
        skyboxManager.AddSmokeAttenuation(10f);
    }

    public override void OnCatastropheEnds() {
        skyboxManager.ClearSmoke();
    }
}