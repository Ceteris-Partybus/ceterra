using Mirror;
using System;
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

        if (uiDocument == null) {
            Debug.LogError("UIDocument is not assigned in CurrentTurnManager!");
            return;
        }

        rootElement = uiDocument.rootVisualElement;
        currentPlayerNameLabel = rootElement.Q<Label>("current-player-name");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        boardButton = rootElement.Q<Button>("board-button");

        if (currentPlayerNameLabel == null) {
            Debug.LogError("currentPlayerNameLabel is null in CurrentTurnManager!");
        }

        if (rollDiceButton == null) {
            Debug.LogError("rollDiceButton is null in CurrentTurnManager!");
            return;
        }

        BoardPlayer currentPlayer = BoardContext.Instance.GetCurrentPlayer();
        if (currentPlayer != null) {
            UpdateCurrentPlayerName(currentPlayer.PlayerName);
            AllowRollDiceButtonFor(currentPlayer.PlayerId);
        }

        rollDiceButton.clicked += OnRollDiceButtonClicked;
        boardButton.clicked += OnBoardButtonClicked;
    }

    private void OnRollDiceButtonClicked() {
        if (BoardContext.Instance == null) {
            Debug.LogError("BoardContext.Instance is null in OnRollDiceButtonClicked!");
            return;
        }

        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            BoardPlayer currentPlayer = BoardContext.Instance.GetCurrentPlayer();
            if (currentPlayer != null) {
                currentPlayer.CmdRollDice();
            } else {
                Debug.LogError("Current player is null in OnRollDiceButtonClicked!");
            }
        } else {
            Debug.Log("Roll dice button clicked, but it's not the player's turn.");
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
            Debug.LogWarning("Local player is null in IsLocalPlayerTurn!");
            return false;
        }

        // Check if the local player's ID matches the current player ID
        bool isTurn = localPlayer.PlayerId == currentPlayerId;
        Debug.Log($"IsLocalPlayerTurn: Local player ID = {localPlayer.PlayerId}, Current player ID = {currentPlayerId}, Result = {isTurn}");
        return isTurn;
    }
}
