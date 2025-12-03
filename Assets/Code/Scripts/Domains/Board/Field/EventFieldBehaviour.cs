using System.Collections;
using UnityEngine;

public class EventFieldBehaviour : FieldBehaviour {
    public override FieldType GetFieldType() => FieldType.EVENT;

    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        BoardContext.Instance.TriggerRandomEvent();
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
    }
}
