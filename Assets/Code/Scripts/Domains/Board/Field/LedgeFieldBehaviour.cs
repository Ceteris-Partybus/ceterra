using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

public class LedgeFieldBehaviour : FieldBehaviour {
    public override FieldType GetFieldType() => FieldType.LEDGE;

    [Server]
    public override IEnumerator OnPlayerCross(BoardPlayer player) {
        Debug.Log($"Player crossed a ledge field.");
        yield return MakePlayerJumpToNextField(player);
    }

    [Server]
    private IEnumerator MakePlayerJumpToNextField(BoardPlayer player) {
        player.PlayerMovement.IsJumping = true;
        player.PlayerMovement.IsMoving = false;
        player.RpcTriggerAnimation(AnimationType.IDLE);
        yield return new WaitForSeconds(.5f);

        player.RpcTriggerAnimation(AnimationType.JUMP);
        yield return new WaitForSeconds(.35f);

        var targetField = GetNextTargetField();
        yield return player.transform.DOJump(targetField.Position, 2f, 1, .9f).WaitForCompletion();
        yield return new WaitForSeconds(.5f);

        player.NormalizedSplinePosition = targetField.NormalizedSplinePosition;
        player.SplineKnotIndex = targetField.SplineKnotIndex;
        player.PlayerMovement.IsJumping = false;

        yield return targetField.OnPlayerCross(player);
    }
}
