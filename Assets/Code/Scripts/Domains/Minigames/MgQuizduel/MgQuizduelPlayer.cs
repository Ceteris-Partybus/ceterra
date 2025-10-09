using Mirror;
using System;
using UnityEngine;

public class MgQuizduelPlayer : SceneConditionalPlayer, IMinigameRewardHandler {
    private int score = 0;
    private bool hasFinishedQuiz = false;
    private int earnedCoinReward = 0;

    public int Score => score;
    public bool HasFinishedQuiz => hasFinishedQuiz;
    public int EarnedCoinReward => earnedCoinReward;

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgQuizduel";
    }

    [Server]
    protected override void OnServerInitialize() {
        score = 0;
        hasFinishedQuiz = false;
        earnedCoinReward = 0;
    }

    [Command]
    public void CmdAddScore(int amount) {
        score += amount;
    }

    [Command]
    public void CmdSetFinishedQuiz() {
        hasFinishedQuiz = true;
    }

    [Server]
    public void SetEarnedCoinReward(int reward) {
        earnedCoinReward = reward;
    }

    public void HandleMinigameRewards(BoardPlayer player) {
        player.PlayerStats.ModifyCoins(Math.Max(0, EarnedCoinReward));
        player.PlayerStats.ModifyScore(Math.Max(0, EarnedCoinReward / 15));
    }
}