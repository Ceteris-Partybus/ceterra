using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class EndBehaviour : NetworkBehaviour {
    [SerializeField] private UIDocument uiDocument;

    [ClientCallback]
    void Start() {
        int groupScore = BoardContext.Instance.EvaluateGlobalScore();
        int playerScore = BoardContext.Instance.GetLocalPlayer().PlayerStats.GetScore();
        int totalScore = playerScore + groupScore;

        var boardPlayers = BoardContext.Instance.GetRemotePlayers();

        Label localPlayerScoreLabel = uiDocument.rootVisualElement.Q<Label>("local-player-score");
        Label groupScoreLabel = uiDocument.rootVisualElement.Q<Label>("group-score");

        localPlayerScoreLabel.text = totalScore.ToString();
        groupScoreLabel.text = groupScore.ToString();

        Debug.Log($"Group score: {groupScore}");
        Debug.Log($"Local player score: {playerScore}");
        Debug.Log($"Total score: {totalScore}");

        foreach (var player in boardPlayers) {
            Debug.Log($"Player {player.PlayerName} score: {player.PlayerStats.GetScore()}");
        }
    }
}