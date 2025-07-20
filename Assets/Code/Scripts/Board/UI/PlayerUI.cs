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

        boardPlayer = GetComponent<BoardPlayer>();

        if (playerCanvas != null) {
            playerCanvas.enabled = isLocalPlayer;
        }
    }

    public override void OnStartLocalPlayer() {

        if (diceButton != null) {

            diceButton.onClick.RemoveAllListeners();

            diceButton.onClick.AddListener(() => {
                if (boardPlayer != null) {
                    boardPlayer.CmdRollDice();
                }
            });

        }
        if (playerCanvas != null) {
            playerCanvas.enabled = true;
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