using Mirror;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentTurnManager : NetworkedSingleton<CurrentTurnManager> {

    //TODO: Die Sachen mit dem Quiz sind nur provisorisch hier drin und werden später entfernt.    
    [SerializeField]
    private GameObject quizGameObject;
    private Button quizDiceButton;
    private BoardquizController boardquizController;

    private void OnQuizButtonClicked() {
        Debug.Log("Quiz-Button geklickt.");

        if (quizGameObject != null && boardquizController != null) {
            var activePlayer = BoardContext.Instance.GetCurrentPlayer();

            if (activePlayer != null) {
                boardquizController.InitializeQuizForPlayer(activePlayer);

                quizGameObject.SetActive(true);
            }
            else {
                Debug.LogError("Konnte das Quiz nicht starten, da kein aktueller Spieler gefunden wurde!");
            }
        }
    }

    private void InitQuizComponents() {
        quizDiceButton = rootElement.Q<Button>("open-quiz-button");
        quizDiceButton.clicked += OnQuizButtonClicked;
        boardquizController = quizGameObject.GetComponent<BoardquizController>();
    }

    // bis hier löschen 

    VisualElement rootElement;
    [SerializeField]
    private UIDocument uiDocument;
    private Label currentPlayerNameLabel;
    private Button rollDiceButton;

    protected override void Start() {
        base.Start();
        rootElement = uiDocument.rootVisualElement;
        currentPlayerNameLabel = rootElement.Q<Label>("current-player-name");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");

        BoardPlayer currentPlayer = BoardContext.Instance.GetCurrentPlayer();
        if (currentPlayer != null) {
            UpdateCurrentPlayerName(currentPlayer.PlayerName);
            AllowRollDiceButtonFor(currentPlayer.PlayerId);
        }

        rollDiceButton.clicked += OnRollDiceButtonClicked;

        InitQuizComponents(); // LÖSCHEN WENN NICHT MEHR BENÖTIGT
    }

    private void OnRollDiceButtonClicked() {
        if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
            BoardContext.Instance.GetCurrentPlayer().CmdRollDice();
        }
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
