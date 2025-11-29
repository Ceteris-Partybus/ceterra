[System.Serializable]
public class MgPlayerRankingData {
    public string playerName;
    public int score;
    public int reward;
    public int rank;

    // Default-Konstruktor für die Serialisierung von Mirror
    public MgPlayerRankingData() {
    }

    public MgPlayerRankingData(string name, int playerScore, int rewardAmount, int playerRank) {
        playerName = name;
        score = playerScore;
        reward = rewardAmount;
        rank = playerRank;
    }
}