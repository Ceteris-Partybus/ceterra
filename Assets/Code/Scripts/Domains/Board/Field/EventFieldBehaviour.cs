using Mirror;
using UnityEngine;
using UnityEngine.Splines;

public class EventFieldBehaviour : FieldBehaviour {
    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        CompleteFieldInvocation();
    }
}
