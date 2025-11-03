using Mirror;

public class MgMemoryPlayer : SceneConditionalPlayer {
    private int score = 0;
    private int earnedCoinReward = 0;

    public int Score => score;
    public int EarnedCoinReward => earnedCoinReward;

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgMemory";
    }

    [Server]
    protected override void OnServerInitialize() {
        score = 0;
        earnedCoinReward = 0;
    }

    [Command]
    public void CmdAddScore(int amount) {
        score += amount;
    }

    [Server]
    public void AddScore(int amount) {
        score += amount;
    }

    [Server]
    public void SetEarnedCoinReward(int reward) {
        earnedCoinReward = reward;
    }
}