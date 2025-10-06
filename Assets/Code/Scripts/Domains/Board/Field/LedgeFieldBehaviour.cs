using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

public class LedgeFieldBehaviour : FieldBehaviour {
    [Server]
    protected override void OnPlayerCross(BoardPlayer player) {
        Debug.Log($"Player crossed a ledge field.");
        StartCoroutine(MakePlayerJumpToNextField(player));
    }

    [Server]
    private IEnumerator MakePlayerJumpToNextField(BoardPlayer player) {
        player.IsJumping = true;
        player.IsMoving = false;
        player.RpcTriggerAnimation(AnimationType.IDLE);
        yield return new WaitForSeconds(.5f);

        player.RpcTriggerAnimation(AnimationType.JUMP);
        yield return new WaitForSeconds(.25f);

        var targetField = GetNextTargetField();
        yield return player.transform.DOJump(targetField.Position, 3f, 1, 1.5f).WaitForCompletion();
        yield return new WaitForSeconds(1f);

        player.NormalizedSplinePosition = targetField.NormalizedSplinePosition;
        player.SplineKnotIndex = targetField.SplineKnotIndex;

        player.RpcTriggerAnimation(AnimationType.RUN);
        player.IsMoving = true;
        player.IsJumping = false;

        CompleteFieldInvocation();
    }
}
