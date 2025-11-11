public class AffectedPlayerData {
    private const long GLOBAL_DAMAGE_TAKEN_TRANSLATION_ID = 65711785694019584;
    private const long DAMAGE_TAKEN_TRANSLATION_ID = 56668768562413568;
    private BoardPlayer player;
    public BoardPlayer Player => player;
    private float distance;
    public float Distance => distance;
    private int inflictedDamage;
    public int InflictedDamage => inflictedDamage;

    public AffectedPlayerData(BoardPlayer player, int inflictedDamage) {
        this.player = player;
        this.distance = -1;
        this.inflictedDamage = inflictedDamage;
    }

    public AffectedPlayerData(BoardPlayer player, float distance, int inflictedDamage) {
        this.player = player;
        this.distance = distance;
        this.inflictedDamage = inflictedDamage;
    }

    public void Deconstruct(out BoardPlayer player, out float distance, out int inflictedDamage) {
        player = Player;
        distance = Distance;
        inflictedDamage = InflictedDamage;
    }

    public override string ToString() {
        if (distance == -1) {
            return LocalizationManager.Instance.GetLocalizedText(GLOBAL_DAMAGE_TAKEN_TRANSLATION_ID, new object[] { Player.PlayerName, InflictedDamage });
        }
        return LocalizationManager.Instance.GetLocalizedText(DAMAGE_TAKEN_TRANSLATION_ID, new object[] { Player.PlayerName, Distance.ToString("F2"), InflictedDamage });
    }
}