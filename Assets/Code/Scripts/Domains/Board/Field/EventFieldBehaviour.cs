using Mirror;
using System.Collections;
using UnityEngine;

public class EventFieldBehaviour : FieldBehaviour {
    protected override void OnFieldInvoked(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        BoardContext.Instance.TriggerRandomEvent();

        // Manually set animation as finished to fulfill the turn completion condition
        player.IsAnimationFinished = true;

        CompleteFieldInvocation();
    }
}
