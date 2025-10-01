using UnityEngine.UIElements;

public class LobbyPlayerSlotUI {
    public VisualElement parent;
    private VisualElement buttonContainer;
    public Label playerDisplayName;
    public Label playerPing;
    public IconWithLabel selectedCharacter;
    public IconWithLabel selectedDice;
    public Button characterSelectionBtn;
    public Button readyBtn;
    private DottedAnimation waitingAnimation;
    private bool isInitialized = false;

    public LobbyPlayerSlotUI(VisualElement parent) {
        this.parent = parent;
        this.buttonContainer = parent.Q<VisualElement>("button-area");

        playerDisplayName = parent.Q<Label>("player-name-label");
        playerPing = parent.Q<Label>("ping-label");

        selectedCharacter = new IconWithLabel(parent.Q<VisualElement>("character-content"), "character-icon", "character-name");
        selectedDice = new IconWithLabel(parent.Q<VisualElement>("dice-content"), "dice-icon", "dice-name");

        characterSelectionBtn = parent.Q<Button>("character-selection-button");
        characterSelectionBtn.clicked += PlayerHud.Instance.ShowCharacterSelection;
        readyBtn = parent.Q<Button>("ready-button");

        waitingAnimation = new DottedAnimation(playerDisplayName, "Waiting for player");
        Clear();
    }

    public void AssignTo(LobbyPlayer lobbyPlayer) {
        if (waitingAnimation.isRunning) { waitingAnimation.Stop(); }

        parent.RemoveFromClassList("empty-slot");

        playerDisplayName.text = lobbyPlayer.PlayerName ?? "New Player";
        playerPing.text = lobbyPlayer.Ping.ToString() + " ms";

        var character = lobbyPlayer?.CurrentCharacterInstance?.GetComponent<Character>();
        if (character != null) {
            selectedCharacter.SetIconAndLabel(character.Icon, character.CharacterName);
        }

        var dice = lobbyPlayer?.CurrentDiceInstance?.GetComponent<Dice>();
        if (dice != null) {
            selectedDice.SetIconAndLabel(dice.Icon, dice.DiceName);
        }

        var isReady = lobbyPlayer.readyToBegin;
        parent.EnableInClassList("ready", isReady);
        readyBtn.EnableInClassList("ready", isReady);
        readyBtn.text = isReady ? "Click to unready" : "Click to ready";
        characterSelectionBtn.SetEnabled(!isReady);

        if (lobbyPlayer.isLocalPlayer && !isInitialized) {
            readyBtn.clicked += () => lobbyPlayer.CmdChangeReadyState(!lobbyPlayer.readyToBegin);
            buttonContainer.Add(characterSelectionBtn);
            buttonContainer.Add(readyBtn);
        }
        isInitialized = true;
    }

    public void Clear() {
        if (waitingAnimation.isRunning) { return; }

        isInitialized = false;
        playerPing.text = "";
        parent.AddToClassList("empty-slot");

        if (buttonContainer.Contains(characterSelectionBtn)) {
            buttonContainer.Remove(characterSelectionBtn);
            buttonContainer.Remove(readyBtn);
        }

        waitingAnimation.Start();
    }
}