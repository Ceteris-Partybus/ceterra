using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MgRewardService : NetworkedSingleton<MgRewardService> {
    public void DistributeRewards() {
        // Finde alle aktiven Spieler, die IMinigameRewardHandler implementieren
        var allActivePlayers = FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(p => p.IsActiveForCurrentScene && p is IMinigameRewardHandler)
            .Cast<IMinigameRewardHandler>()
            .OrderByDescending(p => p.playerScore)
            .ToList();

        var rankings = new List<MgPlayerRankingData>();
        for (var i = 0; i < allActivePlayers.Count; i++) {
            var handler = allActivePlayers[i];
            var player = handler as SceneConditionalPlayer;
            var rank = i + 1;
            var reward = CalculateCoinReward(rank);

            rankings.Add(new MgPlayerRankingData {
                playerName = player.PlayerName,
                score = handler.playerScore,
                reward = reward,
                rank = rank
            });

            handler.SetMinigameReward(reward);
        }
        MgScoreboardController.Instance.ShowScoreboard(rankings);
    }

    private int CalculateCoinReward(int rank) {
        return 100 / (int)Mathf.Pow(2, rank - 1);
    }
}
