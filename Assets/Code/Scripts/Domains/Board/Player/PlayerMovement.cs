using Mirror;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[Serializable]
public class PlayerMovement : NetworkBehaviour {
    #region Player

    [SerializeField]
    private BoardPlayer player;

    #endregion

    #region States
    [SyncVar]
    private bool isMoving = false;
    public bool IsMoving {
        get => isMoving;
        set { isMoving = value; }
    }

    [SyncVar]
    private bool isJumping = false;
    public bool IsJumping {
        get => isJumping;
        set { isJumping = value; }
    }
    #endregion

    #region Input
    [SerializeField] private float moveSpeed;
    [SerializeField] private float movementLerp;
    [SerializeField] private float rotationLerp;
    #endregion

    [Server]
    public void MoveToField(int steps) {
        if (isMoving) { return; }
        StartCoroutine(MoveAlongSplineCoroutine(steps));
    }

    [Server]
    private IEnumerator MoveAlongSplineCoroutine(int steps) {
        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var remainingSteps = steps;

        while (remainingSteps > 0) {
            player.RpcUpdateDiceResultLabel(remainingSteps.ToString());
            var targetField = fieldBehaviourList.Find(player.SplineKnotIndex).GetNextTargetField();
            yield return StartCoroutine(ServerSmoothMoveToKnot(targetField));

            player.SplineKnotIndex = targetField.SplineKnotIndex;
            if (!targetField.SkipStepCount) { remainingSteps--; }

            if (remainingSteps > 0) {
                if (targetField.PausesMovement) {
                    yield return StartCoroutine(EnsureTargetPosition(targetField.Position));
                }
                yield return StartCoroutine(targetField.OnPlayerCross(player));
            }
        }
        yield return StartCoroutine(EnsureTargetPosition(fieldBehaviourList.Find(player.SplineKnotIndex).Position));

        player.RpcTriggerAnimation(AnimationType.IDLE);
        isMoving = false;
        player.RpcHideDiceResultLabel();

        CameraHandler.Instance.RpcZoomIn();
        yield return new WaitForSeconds(CameraHandler.Instance.PlayerToZoomBlendTime);

        var finalField = fieldBehaviourList.Find(player.SplineKnotIndex);
        yield return StartCoroutine(finalField.InvokeOnPlayerLand(player));

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(CameraHandler.Instance.ZoomToPlayerBlendTime);

        BoardContext.Instance.OnPlayerMovementComplete(player);
    }

    [Server]
    private IEnumerator EnsureTargetPosition(Vector3 targetPosition) {
        while (Vector3.Distance(player.transform.position, targetPosition) > .05f) {
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * .5f * Time.deltaTime);
            yield return null;
        }
        player.transform.position = targetPosition;
    }

    [Server]
    private IEnumerator ServerSmoothMoveToKnot(FieldBehaviour targetField) {
        var currentSpline = player.SplineContainer.Splines[player.SplineKnotIndex.Spline];
        var targetSpline = player.SplineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var spline = targetSpline;
        var normalizedTargetPosition = targetField.NormalizedSplinePosition;

        if (!IsMoving) {
            isJumping = false;
            isMoving = true;
            player.RpcTriggerAnimation(AnimationType.RUN);
        }

        if (player.SplineKnotIndex.Spline != targetField.SplineKnotIndex.Spline) {
            if (targetField.SplineKnotIndex.Knot == 1) {
                player.SplineKnotIndex = targetField.SplineKnotIndex;
                player.NormalizedSplinePosition = 0f;
            }
            else {
                normalizedTargetPosition = 1f;
                spline = currentSpline;
            }
        }

        if (targetField.SplineKnotIndex.Knot == 0 && targetSpline.Closed) {
            normalizedTargetPosition = 1f;
        }

        while (player.NormalizedSplinePosition != normalizedTargetPosition) {
            player.NormalizedSplinePosition = Mathf.MoveTowards(player.NormalizedSplinePosition, normalizedTargetPosition, moveSpeed / spline.GetLength() * Time.deltaTime);
            yield return null;
        }

        player.NormalizedSplinePosition = targetField.NormalizedSplinePosition;
    }

    void MoveAndRotate() {
        var movementBlend = Mathf.Pow(0.5f, Time.deltaTime * movementLerp);
        var targetPosition = player.SplineContainer.EvaluatePosition(player.SplineKnotIndex.Spline, player.NormalizedSplinePosition);
        player.transform.position = Vector3.Lerp(targetPosition, player.transform.position, movementBlend);

        if (isMoving) {
            player.SplineContainer.Splines[player.SplineKnotIndex.Spline].Evaluate(player.NormalizedSplinePosition, out float3 _, out float3 direction, out float3 _);
            var worldDirection = player.SplineContainer.transform.TransformDirection(direction);

            if (worldDirection.sqrMagnitude > 0.0001f) {
                player.VisualHandler.SetMovementRotation(Quaternion.LookRotation(worldDirection, Vector3.up), rotationLerp);
            }
        }
    }

    void Update() {
        if (isJumping) { return; }
        if (isMoving) { MoveAndRotate(); return; }
    }
}