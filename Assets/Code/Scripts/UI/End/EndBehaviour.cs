using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class EndBehaviour : NetworkBehaviour {
    [SerializeField] private UIDocument uiDocument;

    public override void OnStartClient() {
        int groupScore = BoardContext.Instance.EvaluateGlobalScore();
        int playerScore = BoardContext.Instance.GetLocalPlayer().Score;
        int totalScore = playerScore + groupScore;

        Label localPlayerScoreLabel = uiDocument.rootVisualElement.Q<Label>("local-player-score");
        Label groupScoreLabel = uiDocument.rootVisualElement.Q<Label>("group-score");

        localPlayerScoreLabel.text = totalScore.ToString();
        groupScoreLabel.text = groupScore.ToString();
    }
}