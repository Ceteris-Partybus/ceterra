using UnityEngine;
using UnityEngine.UIElements;

public class CurrentTurnManager : NetworkedSingleton<CurrentTurnManager> {
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootElement;
    private Label currentRoundLabel;
    private Label currentPlayerNameLabel;
    private Button rollDiceButton;
    private Button boardButton;
    private Button settingsButton;
    private BoardPlayer boardPlayer;

    protected override void Start() {
        rootElement = uiDocument.rootVisualElement;
        currentRoundLabel = rootElement.Q<Label>("current-round-text");
        currentPlayerNameLabel = rootElement.Q<Label>("current-turn-text");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        rollDiceButton.clicked += OnRollDiceButtonClicked;

        boardButton = rootElement.Q<Button>("board-button");
        boardButton.clicked += OnBoardButtonClicked;

        settingsButton = rootElement.Q<Button>("settings-button");
        settingsButton.clicked += SettingsController.Instance.OpenSettingsPanel;

        boardPlayer = BoardContext.Instance.GetLocalPlayer();
        BoardContext.Instance.OnNextPlayerTurn += UpdateTurnUI;
        base.Start();
    }

    private void UpdateTurnUI(BoardPlayer currentPlayer, int currentRound, int maxRounds) {
        currentRoundLabel.text = $"{currentRound} / {maxRounds}";
        currentPlayerNameLabel.text = currentPlayer.PlayerName;

        ShowTurnButtons(currentPlayer.isLocalPlayer);
    }

    private void OnRollDiceButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            boardPlayer.CmdToggleDiceRoll();
            rollDiceButton.text = rollDiceButton.text == "Roll Dice" ? "Cancel Roll" : "Roll Dice";
            boardButton.style.display = IsButtonVisible(boardButton) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    private void OnBoardButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            boardPlayer.CmdToggleBoardOverview();
            boardButton.text = boardButton.text == "View Board" ? "Go back to Player" : "View Board";
            rollDiceButton.style.display = IsButtonVisible(rollDiceButton) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    public void ShowTurnButtons(bool isLocalPlayer) {
        var displayStyle = isLocalPlayer ? DisplayStyle.Flex : DisplayStyle.None;
        rollDiceButton.style.display = displayStyle;
        boardButton.style.display = displayStyle;
    }

    private bool IsButtonVisible(Button button) {
        return button.style.display == DisplayStyle.Flex;
    }
}