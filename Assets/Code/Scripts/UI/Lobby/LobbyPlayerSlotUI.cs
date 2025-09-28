using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPlayerSlotUI {
    private TemplateContainer root;
    public Label playerDisplayName;
    public Label playerPing;
    public Button characterSelectionBtn;
    public Button readyBtn;

    public LobbyPlayerSlotUI() {
        var asset = Resources.Load<VisualTreeAsset>("PlayerSlot");
        root = asset.Instantiate();
        playerDisplayName = root.Q<Label>("PlayerNameLabel");
        playerPing = root.Q<Label>("PingLabel");
        characterSelectionBtn = root.Q<Button>("CustomizeButton");
        readyBtn = root.Q<Button>("ReadyButton");

        playerDisplayName.text = "Waiting for player...";
        playerPing.text = "-- ms";

        root.Remove(characterSelectionBtn);
        root.Remove(readyBtn);
    }

    public void assignSlotTo(LobbyPlayer lobbyPlayer, Action showCharacterSelection) {
        playerDisplayName.text = lobbyPlayer.PlayerName ?? "New Player";
        if (lobbyPlayer.isLocalPlayer) {
            characterSelectionBtn.clicked += showCharacterSelection;
            readyBtn.clicked += OnReadyBtnClicked;
            readyBtn.clicked += () => lobbyPlayer.CmdChangeReadyState(!lobbyPlayer.readyToBegin);
            root.Add(characterSelectionBtn);
            root.Add(readyBtn);
        }
    }

    private void OnReadyBtnClicked() {
        var isReady = readyBtn.ClassListContains("ready");
        readyBtn.text = isReady ? "Not Ready" : "Ready";
        readyBtn.ToggleInClassList("ready");
    }
}