using System.Collections.Generic;
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
    List<PrepPlayerUI> playerObjectList = new();
    int playersCount;
    IPrepScreen context;
    bool gameStarted = false;

    void Start() {
        titleLabel = screenUIDoc.rootVisualElement.Q<Label>("Title");
        descriptionLabel = screenUIDoc.rootVisualElement.Q<Label>("Description");
        controlsLabel = screenUIDoc.rootVisualElement.Q<Label>("Controls");
        screenshotImage = screenUIDoc.rootVisualElement.Q<VisualElement>("Screenshot");
        playerList = screenUIDoc.rootVisualElement.Query<Label>("PlayerName").ToList();
        buttonList = screenUIDoc.rootVisualElement.Query<Button>("readyButton").ToList();
    }

    public void Initialize(IPrepScreen context) {
        titleLabel.text = context.GetTitle();
        descriptionLabel.text = context.GetDescription();
        controlsLabel.text = context.GetControls();
        var players = context.GetPlayers();
        playersCount = players.Count;
        for (int i = 0; i < players.Count; i++) {
            playerList[i].parent.parent.style.display = DisplayStyle.Flex;
            playerList[i].text = players[i].PlayerName;
            playerObjectList.Add(new PrepPlayerUI(buttonList[i], players[i]));
        }
        screenshotImage.style.backgroundImage = new StyleBackground(context.GetScreenshot());
        this.context = context;
    }

    private void OnGUI() {
        if (context == null || gameStarted) {
            return;
        }
        foreach (var player in playerObjectList) {
            if (player.isPlayerReady) {
                player.OnReady();
                continue;
            }
            player.OnNotReady();
        }
        int readyCount = 0;
        foreach (var button in buttonList) {
            if (button.text == "Bereit") {
                readyCount++;
            }
        }
        if (readyCount == playersCount) {
            gameStarted = true;
            Debug.Log("All players ready");
            context.StartGame();
            Destroy(gameObject);
        }
    }
}