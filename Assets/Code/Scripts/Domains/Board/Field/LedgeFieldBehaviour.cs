using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

public class LedgeFieldBehaviour : FieldBehaviour {
    public override FieldType GetFieldType() => FieldType.LEDGE;

    public float jumpPower = 2f; // Default 2.0 for upwards jumps, adjust as needed

    [Server]
    public override IEnumerator OnPlayerCross(BoardPlayer player) {
        Debug.Log($"Player crossed a ledge field.");
        yield return MakePlayerJumpToNextField(player);
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
        yield return ParabolicJump(player.transform, targetField.Position, .9f, jumpPower);
        yield return new WaitForSeconds(.5f);

        player.NormalizedSplinePosition = targetField.NormalizedSplinePosition;
        player.SplineKnotIndex = targetField.SplineKnotIndex;
        player.PlayerMovement.IsJumping = false;

        yield return targetField.OnPlayerCross(player);
    }

    private IEnumerator ParabolicJump(Transform target, Vector3 endPos, float duration, float heightAboveStart) {
        Vector3 startPos = target.position;
        float time = 0;

        float dy = endPos.y - startPos.y;
        float h = Mathf.Max(heightAboveStart, 0.1f);

        // If jumping up, ensure we clear the target height
        if (dy > 0 && h < dy) h = dy + 0.2f;

        float sqrt2h = Mathf.Sqrt(2 * h);
        float sqrtTerm = Mathf.Sqrt(2 * h - 2 * dy);

        // Calculate gravity g and initial vertical velocity v0
        float u = (sqrt2h + sqrtTerm) / duration;
        float g = u * u;
        float v0 = sqrt2h * u;

        while (time < duration) {
            time += Time.deltaTime;
            float t = time;
            if (t > duration) t = duration;

            float linearT = t / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, linearT);

            float y = startPos.y + v0 * t - 0.5f * g * t * t;
            currentPos.y = y;

            target.position = currentPos;
            yield return null;
        }
        target.position = endPos;
    }
}
