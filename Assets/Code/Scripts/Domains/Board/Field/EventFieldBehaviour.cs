using System.Collections;
using UnityEngine;

public class EventFieldBehaviour : FieldBehaviour {
    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        BoardContext.Instance.TriggerRandomEvent();
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
    }
}
