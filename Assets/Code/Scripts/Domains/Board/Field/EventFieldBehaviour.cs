using System.Collections;
using UnityEngine;

public class EventFieldBehaviour : FieldBehaviour {
    protected override void OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        BoardContext.Instance.TriggerRandomEvent();

        StartCoroutine(CompleteAfterDelay());
    }

    private IEnumerator CompleteAfterDelay() {
        yield return new WaitForSeconds(10f);
        CompleteFieldInvocation();
    }
}
