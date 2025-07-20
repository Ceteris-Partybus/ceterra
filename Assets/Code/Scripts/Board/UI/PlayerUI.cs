using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour {
    [Header("UI References")]
    public Canvas playerCanvas;
    public Button diceButton;
    public Text gameStatusText;

    private BoardPlayer boardPlayer;

    void Start() {
        Debug.Log($"PlayerUI Start() - isLocalPlayer: {isLocalPlayer}");

        boardPlayer = GetComponent<BoardPlayer>();
        Debug.Log($"BoardPlayer found: {boardPlayer != null}");

        if (playerCanvas != null) {
            playerCanvas.enabled = isLocalPlayer;
            Debug.Log($"Canvas enabled: {isLocalPlayer}");
        }
        else {
            Debug.LogError("PlayerCanvas is null!");
        }

        if (diceButton != null) {
            Debug.Log($"DiceButton found: {diceButton.name}");
            Debug.Log($"DiceButton active: {diceButton.gameObject.activeInHierarchy}");
            Debug.Log($"DiceButton interactable: {diceButton.interactable}");
        }
        else {
            Debug.LogError("DiceButton is null!");
        }
    }

    public override void OnStartLocalPlayer() {
        Debug.Log("PlayerUI OnStartLocalPlayer() called");

        if (diceButton != null) {
            Debug.Log("Adding button listener...");

            diceButton.onClick.RemoveAllListeners();

            diceButton.onClick.AddListener(() => {
                Debug.Log("Dice button clicked!");
                if (boardPlayer != null) {
                    Debug.Log("Calling CmdRollDice...");
                    boardPlayer.CmdRollDice();
                }
                else {
                    Debug.LogError("BoardPlayer is null when button clicked!");
                }
            });

            Debug.Log("Button listener added successfully");
        }
        else {
            Debug.LogError("DiceButton is null in OnStartLocalPlayer!");
        }

        if (playerCanvas != null) {
            playerCanvas.enabled = true;
            Debug.Log("Canvas enabled for local player");
        }
    }

    void Update() {
        if (!isLocalPlayer) { return; }

        if (diceButton != null && GameManager.Instance != null) {
            bool canRoll = GameManager.Instance.IsPlayerTurn(boardPlayer) &&
                          !boardPlayer.isMoving &&
                          GameManager.Instance.gameState == GameManager.GameState.PlayerTurn;
            diceButton.interactable = canRoll;

            Text buttonText = diceButton.GetComponentInChildren<Text>();
            if (buttonText != null) {
                if (canRoll) {
                    buttonText.text = "Roll Dice";
                }
                else if (boardPlayer.isMoving) {
                    buttonText.text = "Moving...";
                }
                else {
                    buttonText.text = "Wait for turn";
                }
            }
        }

        if (gameStatusText != null && GameManager.Instance != null) {
            switch (GameManager.Instance.gameState) {
                case GameManager.GameState.WaitingForPlayers:
                    gameStatusText.text = $"Waiting for players... ({GameManager.Instance.connectedPlayers}/{GameManager.Instance.minPlayersToStart})";
                    break;
                case GameManager.GameState.PlayerTurn:
                    gameStatusText.text = GameManager.Instance.IsPlayerTurn(boardPlayer) ? "Your turn!" : "Opponent's turn";
                    break;
                case GameManager.GameState.PlayerMoving:
                    gameStatusText.text = "Player moving...";
                    break;
            }
        }
    }
}