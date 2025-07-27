using UnityEngine;
using UnityEngine.Splines;

public class NormalField : Field {
    public NormalField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.NORMAL, splineKnotIndex, position) {
    }

    public override void Invoke(Player player) {
        Debug.Log($"Player {player.playerName} landed on a normal field.");
        Debug.Log($"Player {player.playerName} - Before: Health={player.GetHealth().Item1}, Money={player.GetMoney().Item1}");
        player.AddMoney(10, new FundsDisplay(0));
        player.AddHealth(5);
        Debug.Log($"Player {player.playerName} - After: Health={player.GetHealth().Item1}, Money={player.GetMoney().Item1}");
    }
}