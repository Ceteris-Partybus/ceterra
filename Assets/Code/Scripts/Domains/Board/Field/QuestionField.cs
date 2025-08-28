using UnityEngine;
using UnityEngine.Splines;

public class QuestionField : Field {
    public QuestionField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.QUESTION, splineKnotIndex, position) {
    }

    private const int HEALTHEFFECT = 10;
    private const int MONEYEFFECT = 10;
    private BoardquizController boardquizController;
    private GameObject quizGameObject;

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player landed on a question field.");
        if (quizGameObject != null && boardquizController != null) {
            if (player != null) {
                boardquizController.InitializeQuizForPlayer(player);

                quizGameObject.SetActive(true);
            }
            else {
                Debug.LogError("Konnte das Quiz nicht starten, da kein aktueller Spieler gefunden wurde!");
            }
        }
    }
}