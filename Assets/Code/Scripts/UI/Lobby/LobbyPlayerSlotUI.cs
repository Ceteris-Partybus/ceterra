using System;
using UnityEngine.UIElements;

public class LobbyPlayerSlotUI {
    private LobbyPlayer lobbyPlayer;
    public VisualElement parent;
    private VisualElement container;
    public Label playerDisplayName;
    public Label playerPing;
    public Button characterSelectionBtn;
    public Button readyBtn;
    private IVisualElementScheduledItem waitingAnimation;
    private int dotCount = 0;

    public LobbyPlayerSlotUI(VisualElement parent) {
        this.parent = parent;
        this.container = parent.Q<VisualElement>("player-slot");
        playerDisplayName = parent.Q<Label>("player-name-label");
        playerPing = parent.Q<Label>("ping-label");
        characterSelectionBtn = parent.Q<Button>("character-selection-button");
        readyBtn = parent.Q<Button>("ready-button");

        Clear();
    }

    public void AssignTo(LobbyPlayer lobbyPlayer, Action showCharacterSelection) {
        if (this.lobbyPlayer.index == lobbyPlayer.index) { return; }

        this.lobbyPlayer = lobbyPlayer;
        StopWaitingAnimation();
        playerDisplayName.text = lobbyPlayer.PlayerName ?? "New Player";
        if (lobbyPlayer.isLocalPlayer) {
            characterSelectionBtn.clicked += showCharacterSelection;
            readyBtn.clicked += OnReadyBtnClicked;
            readyBtn.clicked += () => lobbyPlayer.CmdChangeReadyState(!lobbyPlayer.readyToBegin);
            container.Add(characterSelectionBtn);
            container.Add(readyBtn);
        }
    }

    public void Clear() {
        lobbyPlayer = null;
        playerPing.text = "";

        container.Remove(characterSelectionBtn);
        container.Remove(readyBtn);

        StartWaitingAnimation();
    }

    private void OnReadyBtnClicked() {
        var isReady = readyBtn.ClassListContains("ready");
        readyBtn.text = isReady ? "Not Ready" : "Ready";
        readyBtn.ToggleInClassList("ready");
    }

    private void StartWaitingAnimation() {
        StopWaitingAnimation();
        dotCount = 0;
        waitingAnimation = playerDisplayName.schedule.Execute(AnimateWaitingText).Every(500);
    }

    private void StopWaitingAnimation() {
        waitingAnimation?.Pause();
        waitingAnimation = null;
    }

    private void AnimateWaitingText() {
        if (lobbyPlayer != null) { return; }

        dotCount = (dotCount + 1) % 4;
        var dots = new string('.', dotCount);
        playerDisplayName.text = $"Waiting for player{dots}";
    }

    void Update() {
        if (lobbyPlayer == null) { return; }
        playerDisplayName.text = lobbyPlayer.PlayerName ?? "New Player";
        playerPing.text = lobbyPlayer.Ping.ToString() + " ms";
    }
}