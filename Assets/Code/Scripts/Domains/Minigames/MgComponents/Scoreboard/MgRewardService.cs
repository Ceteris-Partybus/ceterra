using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MgRewardService : NetworkedSingleton<MgRewardService> {
    public void DistributeRewards() {
        var allActivePlayers = FindAllActivePlayer();

        var rankings = new List<MgPlayerRankingData>();
        var processedIndices = new HashSet<int>();

        for (var i = 0; i < allActivePlayers.Count; i++) {
            if (processedIndices.Contains(i)) {
                continue;
            }

            var currentScore = allActivePlayers[i].playerScore;

            var tiedIndices = new List<int> { i };
            for (var j = i + 1; j < allActivePlayers.Count; j++) {
                if (allActivePlayers[j].playerScore == currentScore) {
                    tiedIndices.Add(j);
                }
            }

            var totalReward = 0;
            for (var rankOffset = 0; rankOffset < tiedIndices.Count; rankOffset++) {
                totalReward += CalculateCoinReward(i + 1 + rankOffset);
            }
            var sharedReward = totalReward / tiedIndices.Count;

            foreach (var idx in tiedIndices) {
                processedIndices.Add(idx);
                var handler = allActivePlayers[idx];
                var player = handler as SceneConditionalPlayer;

                rankings.Add(new MgPlayerRankingData {
                    playerName = player.PlayerName,
                    score = handler.playerScore,
                    reward = sharedReward,
                    rank = i + 1
                });

                handler.SetMinigameReward(sharedReward);
            }
        }
        MgScoreboardController.Instance.ShowScoreboard(rankings);
    }

    private static List<IMinigameRewardHandler> FindAllActivePlayer() {
        return FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(p => p.IsActiveForCurrentScene && p is IMinigameRewardHandler)
            .Cast<IMinigameRewardHandler>()
            .OrderByDescending(p => p.playerScore)
            .ToList();
    }

    private int CalculateCoinReward(int rank) {
        return 100 / (int)Mathf.Pow(2, rank - 1);
    }
}
