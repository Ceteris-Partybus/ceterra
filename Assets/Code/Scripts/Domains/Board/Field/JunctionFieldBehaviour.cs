using UnityEngine;

public class JunctionFieldBehaviour : FieldBehaviour {
    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player landed on a junction field.");
        CompleteFieldInvocation();
    }
}
