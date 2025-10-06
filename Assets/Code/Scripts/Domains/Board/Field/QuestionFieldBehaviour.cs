using Mirror;
using UnityEngine;

public class QuestionFieldBehaviour : FieldBehaviour {
    [Server]
    protected override void OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
        if (BoardquizController.Instance.gameObject != null && BoardquizController.Instance != null) {
            if (player != null) {
                // Note, that OnQuizClosed must be invoked on server side!
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
        if (BoardquizController.Instance != null) {
            BoardquizController.Instance.OnQuizClosed -= OnQuizCompleted;
        }

        Debug.Log("Completing quiz....");
        CompleteFieldInvocation();
    }
}
