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

    protected override void Start() {
        rootElement = uiDocument.rootVisualElement;
        currentRoundLabel = rootElement.Q<Label>("current-round-text");
        currentPlayerNameLabel = rootElement.Q<Label>("current-turn-text");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        rollDiceButton.clicked += OnRollDiceButtonClicked;

        boardButton = rootElement.Q<Button>("board-button");
        boardButton.clicked += OnBoardButtonClicked;

        settingsButton = rootElement.Q<Button>("settings-button");
        BoardContext.Instance.OnNextPlayerTurn += UpdateTurnUI;
        base.Start();
    }

    private void UpdateTurnUI(BoardPlayer currentPlayer, int currentRound, int maxRounds) {
        currentPlayerNameLabel.text = currentPlayer.PlayerName;
        ShowTurnButtons(currentPlayer.isLocalPlayer);
        UpdateRoundLabel(currentRound, maxRounds);
    }

    private void UpdateRoundLabel(int currentRound, int maxRounds) {
        currentRoundLabel.text = $"{currentRound} / {maxRounds}";
    }

    private void OnRollDiceButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            BoardContext.Instance.GetCurrentPlayer().CmdRollDice();
        }
    }

    private void OnBoardButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            BoardContext.Instance.GetCurrentPlayer().CmdToggleBoardOverview();
        }
    }
    public void ShowTurnButtons(bool isLocalPlayer) {
        var displayStyle = isLocalPlayer ? DisplayStyle.Flex : DisplayStyle.None;
        rollDiceButton.style.display = displayStyle;
        boardButton.style.display = displayStyle;
        rollDiceButton.SetEnabled(isLocalPlayer);
        boardButton.SetEnabled(isLocalPlayer);
    }
}
