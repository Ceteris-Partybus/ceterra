using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class JunctionFieldBehaviour : FieldBehaviour {
    [Header("Branch Arrows")]
    [SerializeField] private Transform branchArrowPrefab;
    [SerializeField] private float branchArrowRadius;
    private List<GameObject> branchArrows = new List<GameObject>();

    [SyncVar]
    private bool isWaitingForBranchChoice = false;
    private FieldBehaviour targetField;
    private BoardPlayer crossingPlayer;

    public override FieldBehaviour GetNextTargetField() {
        return targetField ?? base.GetNextTargetField();
    }

    [Server]
    protected override void OnPlayerCross(BoardPlayer player) {
        Debug.Log($"Player crossed a junction field.");
        this.crossingPlayer = player;
        StartCoroutine(LetPlayerChoosePath());
    }

    [Server]
    private IEnumerator LetPlayerChoosePath() {
        crossingPlayer.IsMoving = false;
        crossingPlayer.RpcTriggerAnimation(AnimationType.JUNCTION_ENTRY);
        isWaitingForBranchChoice = true;
        crossingPlayer.TargetShowBranchArrows(this);
        yield return new WaitUntil(() => !isWaitingForBranchChoice);

        crossingPlayer.RpcTriggerAnimation(AnimationType.RUN);
        crossingPlayer.IsMoving = true;

        CompleteFieldInvocation();
    }

    [Client]
    public void ShowBranchArrows(BoardPlayer player) {
        for (var i = 0; i < nextFields.Count; i++) {
            var branchArrow = InstantiateBranchArrow(nextFields[i], player);
            branchArrow.GetComponent<BranchArrowMouseEventHandler>()?.Initialize(this, i);
            branchArrows.Add(branchArrow);
        }
    }

    [Client]
    private GameObject InstantiateBranchArrow(FieldBehaviour targetField, BoardPlayer player) {
        var targetSpline = player.SplineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var normalizedPlayerPosition = player.NormalizedSplinePosition;

        if (player.SplineKnotIndex.Spline != targetField.SplineKnotIndex.Spline && targetField.SplineKnotIndex.Knot == 1) {
            normalizedPlayerPosition = 0f;
        }

        var offset = 0.01f;
        var tangent = targetSpline.EvaluateTangent(normalizedPlayerPosition + offset);
        var worldTangent = player.SplineContainer.transform.TransformDirection(tangent).normalized;

        var branchArrowPosition = transform.position + worldTangent * branchArrowRadius;
        return Instantiate(branchArrowPrefab.gameObject, branchArrowPosition, Quaternion.LookRotation(worldTangent, Vector3.up));
    }

    [Client]
    public void HideBranchArrows() {
        foreach (var arrow in branchArrows) {
            Destroy(arrow);
        }
        branchArrows.Clear();
    }

    [Command(requiresAuthority = false)]
    public void CmdChooseBranchPath(int pathIndex) {
        if (!isWaitingForBranchChoice || pathIndex < 0 || pathIndex >= nextFields.Count) { return; }

        targetField = nextFields[pathIndex];
        isWaitingForBranchChoice = false;
        crossingPlayer.TargetHideBranchArrows(this);
    }
}
