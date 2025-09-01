using Mirror;
using UnityEngine;
using UnityEngine.Splines;

public class QuestionFieldBehaviour : FieldBehaviour {
    [Server]
    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
        if (BoardquizController.Instance.gameObject != null && BoardquizController.Instance != null) {
            if (player != null) {
                // Subscribe to quiz completion event
                BoardquizController.Instance.OnQuizClosed += OnQuizCompleted;
                BoardContext.Instance.ShowQuizForPlayer(player.PlayerId);
            }
            else {
                Debug.LogError("Konnte das Quiz nicht starten, da kein aktueller Spieler gefunden wurde!");
                CompleteFieldInvocation();
            }
        }
        else {
            Debug.LogError("BoardquizController not found!");
            CompleteFieldInvocation();
        }
    }

    private void OnQuizCompleted() {
        // Unsubscribe from the event
        if (BoardquizController.Instance != null) {
            BoardquizController.Instance.OnQuizClosed -= OnQuizCompleted;
        }

        // Signal that this field's action is complete
        Debug.Log("Completing quiz....");
        CompleteFieldInvocation();
    }
}
