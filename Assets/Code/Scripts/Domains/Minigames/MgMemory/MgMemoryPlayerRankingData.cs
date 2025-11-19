[System.Serializable]
public class MgMemoryPlayerRankingData {
    public string playerName;
    public int score;
    public int reward;
    public int rank;

    // Default-Konstruktor für die Serialisierung von Mirror
    public MgMemoryPlayerRankingData() {
    }

    public MgMemoryPlayerRankingData(string name, int playerScore, int rewardAmount, int playerRank) {
        playerName = name;
        score = playerScore;
        reward = rewardAmount;
        rank = playerRank;
    }

    public static MgMemoryPlayerRankingData FromPlayer(MgMemoryPlayer player, int rank) {
        return new MgMemoryPlayerRankingData(
            player.PlayerName,
            player.Score,
            player.EarnedCoinReward,
            rank
        );
    }
}