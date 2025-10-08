using UnityEngine;
using UnityEngine.UIElements;

public class CurrentTurnManager : NetworkedSingleton<CurrentTurnManager> {

    VisualElement rootElement;
    [SerializeField]
    private UIDocument uiDocument;
    private Label currentPlayerNameLabel;
    private Button rollDiceButton;
    private Button boardButton;

    protected override void Start() {
        base.Start();
        rootElement = uiDocument.rootVisualElement;
        currentPlayerNameLabel = rootElement.Q<Label>("current-player-name");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        boardButton = rootElement.Q<Button>("board-button");

        BoardPlayer currentPlayer = BoardContext.Instance.GetCurrentPlayer();
        if (currentPlayer != null) {
            UpdateCurrentPlayerName(currentPlayer.PlayerName);
            AllowRollDiceButtonFor(currentPlayer.PlayerId);
        }

        rollDiceButton.clicked += OnRollDiceButtonClicked;
        boardButton.clicked += OnBoardButtonClicked;
    }

    private void OnRollDiceButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            BoardContext.Instance.GetCurrentPlayer().CmdRollDice();
        }
    }

    private void OnBoardButtonClicked() {
        BoardContext.Instance.GetCurrentPlayer().CmdToggleBoardOverview();
    }

    public void UpdateCurrentPlayerName(string playerName) {
        if (currentPlayerNameLabel != null) {
            currentPlayerNameLabel.text = playerName;
        }
    }

    public void AllowRollDiceButtonFor(int playerId) {
        if (rollDiceButton != null) {
            // Check if the local player is the current player
            bool isLocalPlayerTurn = IsLocalPlayerTurn(playerId);
            rollDiceButton.SetEnabled(isLocalPlayerTurn);
            boardButton.SetEnabled(isLocalPlayerTurn);
            Debug.Log($"Setting button enabled to {isLocalPlayerTurn} for local player");
        }
    }

    private bool IsLocalPlayerTurn(int currentPlayerId) {
        // Find the local player's BoardPlayer component
        BoardPlayer localPlayer = BoardContext.Instance.GetLocalPlayer();
        if (localPlayer == null) {
            return false;
        }

        // Check if the local player's ID matches the current player ID
        return localPlayer.PlayerId == currentPlayerId;
    }
}
