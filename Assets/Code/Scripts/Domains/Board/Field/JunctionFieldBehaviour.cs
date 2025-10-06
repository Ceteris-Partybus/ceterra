using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class JunctionFieldBehaviour : FieldBehaviour {
    [Header("Branch Arrows")]
    [SerializeField] private Transform branchArrowPrefab;
    [SerializeField] private float branchArrowRadius;
    private List<GameObject> branchArrows = new List<GameObject>();

    [SyncVar]
    private bool isWaitingForBranchChoice = false;
    public bool IsWaitingForBranchChoice => isWaitingForBranchChoice;
    private FieldBehaviour targetField;
    private BoardPlayer crossingPlayer;

    public override FieldBehaviour GetNextTargetField() {
        return targetField ?? base.GetNextTargetField();
    }

    [Server]
    protected override void OnPlayerCross(BoardPlayer crossingPlayer) {
        Debug.Log($"Player crossed a junction field.");
        this.crossingPlayer = crossingPlayer;
        netIdentity.AssignClientAuthority(crossingPlayer.connectionToClient);

        TargetSetCrossingPlayer(crossingPlayer);
        StartCoroutine(LetPlayerChoosePath());
    }

    [Server]
    private IEnumerator LetPlayerChoosePath() {
        crossingPlayer.IsMoving = false;
        crossingPlayer.RpcTriggerAnimation(AnimationType.JUNCTION_ENTRY);
        isWaitingForBranchChoice = true;
        TargetShowBranchArrows();
        yield return new WaitUntil(() => !isWaitingForBranchChoice);

        netIdentity.RemoveClientAuthority();
        CompleteFieldInvocation();
    }

    [TargetRpc]
    public void TargetShowBranchArrows() {
        for (var i = 0; i < Next.Count; i++) {
            var branchArrow = InstantiateBranchArrow(Next[i]);
            branchArrow.GetComponent<BranchArrowMouseEventHandler>()?.Initialize(this, i);
            branchArrows.Add(branchArrow);
        }
    }

    [Client]
    private GameObject InstantiateBranchArrow(FieldBehaviour targetField) {
        var targetSpline = crossingPlayer.SplineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var normalizedPlayerPosition = crossingPlayer.NormalizedSplinePosition;

        if (crossingPlayer.SplineKnotIndex.Spline != targetField.SplineKnotIndex.Spline && targetField.SplineKnotIndex.Knot == 1) {
            normalizedPlayerPosition = 0f;
        }

        var offset = 0.01f;
        var tangent = targetSpline.EvaluateTangent(normalizedPlayerPosition + offset);
        var worldTangent = crossingPlayer.SplineContainer.transform.TransformDirection(tangent).normalized;

        var branchArrowPosition = transform.position + worldTangent * branchArrowRadius;
        return Instantiate(branchArrowPrefab.gameObject, branchArrowPosition, Quaternion.LookRotation(worldTangent, Vector3.up));
    }

    [Command(requiresAuthority = false)]
    public void CmdChooseBranchPath(int pathIndex) {
        if (!isWaitingForBranchChoice || pathIndex < 0 || pathIndex >= Next.Count) { return; }
        targetField = Next[pathIndex];
        isWaitingForBranchChoice = false;
        TargetHideBranchArrows();
    }

    [TargetRpc]
    public void TargetHideBranchArrows() {
        foreach (var arrow in branchArrows) {
            Destroy(arrow);
        }
        branchArrows.Clear();
    }

    [TargetRpc]
    private void TargetSetCrossingPlayer(BoardPlayer crossingPlayer) {
        this.crossingPlayer = crossingPlayer;
    }
}
