using UnityEngine.UIElements;

public class LobbyPlayerSlotUI {
    private VisualElement parent;
    private VisualElement buttonContainer;
    private Label playerDisplayName;
    private Label playerPing;
    private IconWithLabel selectedCharacter;
    private IconWithLabel selectedDice;
    private Button characterSelectionBtn;
    private Button readyBtn;
    private Label readyStatusDisplayLabel;
    private DottedAnimation waitingAnimation;
    private bool isInitialized = false;

    public LobbyPlayerSlotUI(VisualElement parent) {
        this.parent = parent;
        this.buttonContainer = parent.Q<VisualElement>("button-area");

        playerDisplayName = parent.Q<Label>("player-name-label");
        playerPing = parent.Q<Label>("ping-label");

        selectedCharacter = new IconWithLabel(parent.Q<VisualElement>("character-selection"), "character-icon", "character-name");
        selectedDice = new IconWithLabel(parent.Q<VisualElement>("dice-selection"), "dice-icon", "dice-name");

        characterSelectionBtn = parent.Q<Button>("character-selection-button");
        readyBtn = parent.Q<Button>("ready-button");
        readyStatusDisplayLabel = parent.Q<Label>("ready-status-display-label");

        characterSelectionBtn.clicked += PlayerHud.Instance.ShowCharacterSelection;
        characterSelectionBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();
        readyBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();

        waitingAnimation = new DottedAnimation(playerDisplayName, LocalizationManager.Instance.GetLocalizedText(61452920784519168));
        Clear();
    }

    public void AssignTo(LobbyPlayer lobbyPlayer) {
        if (waitingAnimation.isRunning) { waitingAnimation.Stop(); }

        parent.RemoveFromClassList("empty-slot");

        playerDisplayName.text = lobbyPlayer.PlayerName ?? LocalizationManager.Instance.GetLocalizedText(61453271277338624);
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
        var isReadyText = LocalizationManager.Instance.GetLocalizedText(61453348007936000);
        var isNotReadyText = LocalizationManager.Instance.GetLocalizedText(61453405545398272);
        readyStatusDisplayLabel.text = isReady ? isReadyText : isNotReadyText;
        readyStatusDisplayLabel.parent.EnableInClassList("ready", isReady);
        readyStatusDisplayLabel.parent.style.display = !lobbyPlayer.isLocalPlayer ? DisplayStyle.Flex : DisplayStyle.None;
        parent.EnableInClassList("ready", isReady);
        readyBtn.EnableInClassList("ready", isReady);
        var clickToReadyText = LocalizationManager.Instance.GetLocalizedText(61453992487911424);
        var clickToUnreadyText = LocalizationManager.Instance.GetLocalizedText(61453481038675968);
        readyBtn.text = isReady ? clickToUnreadyText : clickToReadyText;
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
        playerDisplayName.text = "";
        selectedCharacter.Reset();
        selectedDice.Reset();
        readyStatusDisplayLabel.parent.style.display = DisplayStyle.None;
        parent.AddToClassList("empty-slot");

        if (buttonContainer.Contains(characterSelectionBtn)) {
            buttonContainer.Remove(characterSelectionBtn);
            buttonContainer.Remove(readyBtn);
        }

        waitingAnimation.Start();
    }
}