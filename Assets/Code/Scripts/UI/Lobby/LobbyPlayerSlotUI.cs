using UnityEngine.UIElements;

public class LobbyPlayerSlotUI {
    public VisualElement parent;
    private VisualElement container;
    public Label playerDisplayName;
    public Label playerPing;
    public Button characterSelectionBtn;
    public Button readyBtn;
    private IVisualElementScheduledItem waitingAnimation;
    private int dotCount = 0;
    private bool isInitialized = false;

    public LobbyPlayerSlotUI(VisualElement parent) {
        this.parent = parent;
        this.container = parent.Q<VisualElement>("player-slot");

        playerDisplayName = parent.Q<Label>("player-name-label");
        playerPing = parent.Q<Label>("ping-label");
        characterSelectionBtn = parent.Q<Button>("character-selection-button");
        readyBtn = parent.Q<Button>("ready-button");
        characterSelectionBtn.clicked += PlayerHud.Instance.ShowCharacterSelection;
        Clear();
    }

    public void AssignTo(LobbyPlayer lobbyPlayer) {
        if (waitingAnimation != null) { StopWaitingAnimation(); }
        parent.RemoveFromClassList("empty-slot");

        playerDisplayName.text = lobbyPlayer.PlayerName ?? "New Player";
        playerPing.text = lobbyPlayer.Ping.ToString() + " ms";

        var isReady = lobbyPlayer.readyToBegin;
        parent.EnableInClassList("ready", isReady);
        readyBtn.EnableInClassList("ready", isReady);
        readyBtn.text = isReady ? "Click to unready" : "Click to ready";
        characterSelectionBtn.SetEnabled(!isReady);

        if (lobbyPlayer.isLocalPlayer && !isInitialized) {
            readyBtn.clicked += () => lobbyPlayer.CmdChangeReadyState(!lobbyPlayer.readyToBegin);
            container.Add(characterSelectionBtn);
            container.Add(readyBtn);
        }
        isInitialized = true;
    }

    public void Clear() {
        if (waitingAnimation != null) { return; }

        isInitialized = false;
        playerPing.text = "";
        parent.AddToClassList("empty-slot");

        if (container.Contains(characterSelectionBtn)) {
            container.Remove(characterSelectionBtn);
            container.Remove(readyBtn);
        }

        StartWaitingAnimation();
    }

    private void StartWaitingAnimation() {
        dotCount = 0;
        waitingAnimation = playerDisplayName.schedule.Execute(AnimateWaitingText).Every(500);
    }

    private void StopWaitingAnimation() {
        waitingAnimation?.Pause();
        waitingAnimation = null;
    }

    private void AnimateWaitingText() {
        dotCount = (dotCount + 1) % 4;
        var dots = new string('.', dotCount);
        playerDisplayName.text = $"Waiting for player{dots}";
    }
}