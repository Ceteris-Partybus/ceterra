using UnityEngine;

public class LedgeFieldBehaviour : FieldBehaviour {
    protected override void OnPlayerCross(BoardPlayer player) {
        Debug.Log($"Player crossed a ledge field.");
        CompleteFieldInvocation();
    }
}
