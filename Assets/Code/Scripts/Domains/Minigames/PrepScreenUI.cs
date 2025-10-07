using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PrepScreenUI : MonoBehaviour {
    [SerializeField] private UIDocument screenUIDoc;
    Label titleLabel;
    Label descriptionLabel;
    Label controlsLabel;
    VisualElement screenshotImage;
    List<Label> playerList;
    List<Button> buttonList;
    bool allPlayersReady = false;

    void Start() {
        titleLabel = screenUIDoc.rootVisualElement.Q<Label>("Title");
        descriptionLabel = screenUIDoc.rootVisualElement.Q<Label>("Description");
        controlsLabel = screenUIDoc.rootVisualElement.Q<Label>("Controls");
        screenshotImage = screenUIDoc.rootVisualElement.Q<VisualElement>("Screenshot");
        playerList = screenUIDoc.rootVisualElement.Query<Label>("PlayerName").ToList();
        buttonList = screenUIDoc.rootVisualElement.Query<Button>("readyButton").ToList();
        foreach (var button in buttonList) {
            button.clicked += () => ButtonClicked(button);
        }
    }

    public void Initialize(IPrepScreen context) {
        titleLabel.text = context.GetTitle();
        descriptionLabel.text = context.GetDescription();
        controlsLabel.text = context.GetControls();
        var players = context.GetPlayers();
        for (int i = 0; i < players.Count; i++) {
            playerList[i].style.display = DisplayStyle.Flex;
            playerList[i].text = players[i].PlayerName;
        }
        screenshotImage.style.backgroundImage = new StyleBackground(context.GetScreenshot());
    }

    public void ButtonClicked(Button clickedButton) {
        if (clickedButton.text == "Nicht bereit") {
            clickedButton.text = "Bereit";
            clickedButton.RemoveFromClassList("ready-button.not-ready");
            clickedButton.AddToClassList(".ready-button");
            foreach (var button in buttonList) {
                if (button.text == "Nicht bereit") {
                    allPlayersReady = false;
                    break;
                }
                else {
                    allPlayersReady = true;
                }
            }
        }
        else {
            clickedButton.text = "Nicht bereit";
            clickedButton.RemoveFromClassList("ready-button");
            clickedButton.AddToClassList(".ready-button.not-ready");
        }
    }

    public void WaitForPlayers() {
        while (!allPlayersReady) {
            yield return new WaitForSeconds(1);
        }
    }
}