using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PrepScreenUI : MonoBehaviour {
    [SerializeField] private UIDocument screenUIDoc;
    Label titleLabel;
    Label descriptionLabel;
    Label controlsLabel;
    VisualElement screenshotImage;
    List<VisualElement> playerList;
    List<PrepPlayerUI> playerObjectList = new();
    int playersCount;
    IPrepScreen context;
    bool allPlayersReady = false;

    void Start() {
        titleLabel = screenUIDoc.rootVisualElement.Q<Label>("Title");
        descriptionLabel = screenUIDoc.rootVisualElement.Q<Label>("Description");
        controlsLabel = screenUIDoc.rootVisualElement.Q<Label>("Controls");
        screenshotImage = screenUIDoc.rootVisualElement.Q<VisualElement>("Screenshot");
        playerList = screenUIDoc.rootVisualElement.Query<VisualElement>("player-prep-slot").ToList();
    }

    public void Initialize(IPrepScreen context) {
        titleLabel.text = context.GetTitle();
        descriptionLabel.text = context.GetDescription();
        controlsLabel.text = context.GetControls();
        var players = context.GetPlayers();
        playersCount = players.Count;
        var toClear = new List<int> { 0, 1, 2, 3 };
        for (var i = 0; i < players.Count; i++) {
            playerObjectList.Add(new PrepPlayerUI(playerList[i], players[i]));
            toClear.Remove(i);
        }
        foreach (var indexToClear in toClear) {
            playerList[indexToClear].style.display = DisplayStyle.None;
        }
        screenshotImage.style.backgroundImage = new StyleBackground(context.GetScreenshot());
        this.context = context;
    }

    private void Update() {
        if (context == null || allPlayersReady) {
            return;
        }
        var readyCount = 0;
        foreach (var player in playerObjectList) {
            player.UpdateMe();
            readyCount += player.isPlayerReady ? 1 : 0;
        }

        if (readyCount == playersCount) {
            allPlayersReady = true;
            context.StartGame();
            Destroy(gameObject);
        }
    }
}