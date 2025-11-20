using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class QuestionFieldBehaviour : FieldBehaviour {
    private bool quizCompleted = true;

    public override FieldType GetFieldType() => FieldType.QUESTION;

    [Server]
    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        var onQuizCompleted = new Action(() => quizCompleted = true);
        if (BoardquizController.Instance.gameObject != null && BoardquizController.Instance != null) {
            if (player != null) {
                // Note, that OnQuizClosed must be invoked on server side!
                quizCompleted = false;
                BoardquizController.Instance.OnQuizClosed += onQuizCompleted;
                BoardContext.Instance.ShowQuizForPlayer(player.PlayerId);
            }
        }
        yield return new WaitUntil(() => quizCompleted);
        BoardquizController.Instance.OnQuizClosed -= onQuizCompleted;
    }
}
