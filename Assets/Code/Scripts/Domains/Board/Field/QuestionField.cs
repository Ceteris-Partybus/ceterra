using Mirror;
using UnityEngine;
using UnityEngine.Splines;

public class QuestionField : Field {
    public QuestionField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.QUESTION, splineKnotIndex, position) {
    }

    private const int HEALTHEFFECT = 10;
    private const int MONEYEFFECT = 10;

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
        if (BoardquizController.Instance.gameObject != null && BoardquizController.Instance != null) {
            if (player != null) {
                BoardContext.Instance.ShowQuizForPlayer(player.PlayerId);
            }
            else {
                Debug.LogError("Konnte das Quiz nicht starten, da kein aktueller Spieler gefunden wurde!");
            }
        }
    }
}