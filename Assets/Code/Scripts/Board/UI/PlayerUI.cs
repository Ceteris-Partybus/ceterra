using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour {
    [Header("UI References")]
    public Canvas playerCanvas;
    public Button diceButton;
    public Text gameStatusText;

    private Player boardPlayer;

    void Start() {

        boardPlayer = GetComponent<Player>();

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

        if (diceButton != null) {
            bool canRoll = BoardContext.Instance.IsPlayerTurn(boardPlayer) &&
                          !boardPlayer.isMoving &&
                          GameManager.Instance.CurrentState == GameManager.State.ON_BOARD &&
                          BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN;
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

        if (gameStatusText != null) {
            if (GameManager.Instance.CurrentState == GameManager.State.WAITING_FOR_PLAYERS) {
                gameStatusText.text = $"Waiting for players... ({GameManager.Instance.connectedPlayers}/{GameManager.Instance.MinPlayersToStart})";
            }
            else if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_TURN) {
                gameStatusText.text = BoardContext.Instance.IsPlayerTurn(boardPlayer) ? "Your turn!" : "Opponent's turn";
            }
            else if (BoardContext.Instance.CurrentState == BoardContext.State.PLAYER_MOVING) {
                gameStatusText.text = "Player moving...";
            }
        }
    }
}