using UnityEngine;
using UnityEngine.UIElements;

public class CurrentTurnManager : NetworkedSingleton<CurrentTurnManager> {
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootElement;
    private Label currentPlayerNameLabel;
    private Button rollDiceButton;
    private Button boardButton;

    protected override void Start() {
        rootElement = uiDocument.rootVisualElement;
        currentPlayerNameLabel = rootElement.Q<Label>("current-player-name");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        rollDiceButton.clicked += OnRollDiceButtonClicked;

        boardButton = rootElement.Q<Button>("board-button");
        boardButton.clicked += OnBoardButtonClicked;
        BoardContext.Instance.OnNewRoundStarted += UpdateTurnUI;
        base.Start();
    }

    private void UpdateTurnUI(BoardPlayer currentPlayer) {
        var nameDisplay = currentPlayer.isLocalPlayer ? "your" : $"{currentPlayer.PlayerName}'s";
        currentPlayerNameLabel.text = $"It's {nameDisplay} turn!";
        ShowTurnButtons(currentPlayer.isLocalPlayer);
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
