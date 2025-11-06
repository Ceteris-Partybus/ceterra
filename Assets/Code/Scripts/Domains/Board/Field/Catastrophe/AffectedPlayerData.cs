public class AffectedPlayerData {
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
        return LocalizationManager.Instance.GetLocalizedText(56668768562413568, new object[] { Player.PlayerName, Distance.ToString("F2"), InflictedDamage });
    }
}