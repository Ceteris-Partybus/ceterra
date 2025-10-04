using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PrepScreenUI : MonoBehaviour {
    [SerializeField] private UIDocument screenUIDoc;
    Label titleLabel;
    Label descriptionLabel;
    Label controlsLabel;
    Label player1NameLabel;
    Label player2NameLabel;
    Label player3NameLabel;
    Label player4NameLabel;

    void Start() {
        titleLabel = screenUIDoc.rootVisualElement.Q<Label>("Title");
        descriptionLabel = screenUIDoc.rootVisualElement.Q<Label>("Description");
        controlsLabel = screenUIDoc.rootVisualElement.Q<Label>("Controls");
        player1NameLabel = screenUIDoc.rootVisualElement.Q<Label>("Player1");
        player2NameLabel = screenUIDoc.rootVisualElement.Q<Label>("Player2");
        player3NameLabel = screenUIDoc.rootVisualElement.Q<Label>("Player3");
        player4NameLabel = screenUIDoc.rootVisualElement.Q<Label>("Player4");
    }

    public void Initialize(IPrepScreen context) {
        titleLabel.text = context.GetTitle();
        descriptionLabel.text = context.GetDescription();
        controlsLabel.text = context.GetControls();
        var players = context.GetPlayers();
        switch (players.Count) { //Sehr Hässlich, sollte besser gemacht werden
            case 1:
                player1NameLabel.text = players[0].PlayerName;
                break;
            case 2:
                player1NameLabel.text = players[0].PlayerName;
                player2NameLabel.text = players[1].PlayerName;
                break;
            case 3:
                player1NameLabel.text = players[0].PlayerName;
                player2NameLabel.text = players[1].PlayerName;
                player3NameLabel.text = players[2].PlayerName;
                break;
            case 4:
                player1NameLabel.text = players[0].PlayerName;
                player2NameLabel.text = players[1].PlayerName;
                player3NameLabel.text = players[2].PlayerName;
                player4NameLabel.text = players[3].PlayerName;
                break;
        }
        //screenshotImage.sprite = context.GetScreenshot();
    }
}