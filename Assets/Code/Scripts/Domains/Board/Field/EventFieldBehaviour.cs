using System.Collections;
using UnityEngine;

public class EventFieldBehaviour : FieldBehaviour {
    public override FieldType GetFieldType() => FieldType.EVENT;

    protected override void OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        BoardContext.Instance.TriggerRandomEvent();

        StartCoroutine(CompleteAfterDelay());
    }

    private IEnumerator CompleteAfterDelay() {
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
        CompleteFieldInvocation();
    }
}
