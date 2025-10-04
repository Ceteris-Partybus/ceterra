[System.Serializable]
public class MgQuizduelPlayerRankingData {
    public string playerName;
    public int score;
    public int reward;
    public int rank;

    // Default-Konstruktor für die Serialisierung von Mirror
    public MgQuizduelPlayerRankingData() {
    }

    public MgQuizduelPlayerRankingData(string name, int playerScore, int rewardAmount, int playerRank) {
        playerName = name;
        score = playerScore;
        reward = rewardAmount;
        rank = playerRank;
    }

    public static MgQuizduelPlayerRankingData FromPlayer(MgQuizduelPlayer player, int rank) {
        return new MgQuizduelPlayerRankingData(
            player.PlayerName,
            player.Score,
            player.EarnedCoinReward,
            rank
        );
    }
}
