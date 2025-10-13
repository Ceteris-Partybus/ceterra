using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EndBehaviour : NetworkBehaviour {
    [SerializeField] private UIDocument uiDocument;

    [ClientCallback]
    void Start() {
        int groupScore = BoardContext.Instance.EvaluateGlobalScore();
        var localPlayer = BoardContext.Instance.GetLocalPlayer();
        int localPlayerScore = localPlayer.PlayerStats.GetScore();
        int localPlayerTotalScore = localPlayerScore + groupScore;

        var remotePlayers = BoardContext.Instance.GetRemotePlayers();

        Label localPlayerScoreLabel = uiDocument.rootVisualElement.Q<Label>("local-player-score");
        Label groupScoreLabel = uiDocument.rootVisualElement.Q<Label>("group-score");
        VisualElement otherPlayersContainer = uiDocument.rootVisualElement.Q<VisualElement>("other-players-container");

        groupScoreLabel.text = groupScore.ToString();
        localPlayerScoreLabel.text = localPlayerTotalScore.ToString();

        var playerScores = new List<(string name, int totalScore)>();

        foreach (var player in remotePlayers) {
            int playerIndividualScore = player.PlayerStats.GetScore();
            int playerTotalScore = playerIndividualScore + groupScore;
            playerScores.Add((player.PlayerName, playerTotalScore));
        }

        playerScores = playerScores.OrderByDescending(p => p.totalScore).ToList();

        foreach (var (name, totalScore) in playerScores) {
            VisualElement playerRow = new VisualElement();
            playerRow.AddToClassList("player-row");
            playerRow.style.flexDirection = FlexDirection.Row;
            playerRow.style.fontSize = 60;
            playerRow.style.marginBottom = 10;

            Label playerNameLabel = new Label(name + ":");
            playerNameLabel.style.marginRight = 30;
            playerNameLabel.style.minWidth = 200;
            playerNameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);

            Label playerScoreLabel = new Label(totalScore.ToString());
            playerScoreLabel.style.color = new Color(0.5f, 0.8f, 1.0f); // Light blue

            playerRow.Add(playerNameLabel);
            playerRow.Add(playerScoreLabel);
            otherPlayersContainer.Add(playerRow);
        }
    }
}