using Mirror;

public class MgMemoryPlayer : SceneConditionalPlayer {
    private int score = 0;
    private int earnedCoinReward = 0;
    private int firstSelectedCardIndex = -1;
    private int secondSelectedCardIndex = -1;

    public int Score => score;
    public int EarnedCoinReward => earnedCoinReward;
    public int FirstSelectedCardIndex => firstSelectedCardIndex;
    public int SecondSelectedCardIndex => secondSelectedCardIndex;
    public bool HasFirstSelection => firstSelectedCardIndex != -1;
    public bool HasSecondSelection => secondSelectedCardIndex != -1;

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgMemory";
    }

    [Server]
    protected override void OnServerInitialize() {
        score = 0;
        earnedCoinReward = 0;
        firstSelectedCardIndex = -1;
        secondSelectedCardIndex = -1;
    }

    [Command]
    public void CmdAddScore(int amount) {
        score += amount;
    }

    public void AddScore(int amount) {
        score += amount;
    }

    [Server]
    public void SetEarnedCoinReward(int reward) {
        earnedCoinReward = reward;
    }
    public void SetFirstSelectedCard(int cardIndex) {
        firstSelectedCardIndex = cardIndex;
    }

    public void SetSecondSelectedCard(int cardIndex) {
        secondSelectedCardIndex = cardIndex;
    }

    //[Server]
    public void ClearCardSelections() {
        firstSelectedCardIndex = -1;
        secondSelectedCardIndex = -1;
    }
}