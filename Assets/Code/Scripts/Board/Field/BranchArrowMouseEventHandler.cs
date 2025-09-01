using UnityEngine;

public class BranchArrowMouseEventHandler : MonoBehaviour {
    private BoardPlayer boardPlayer;
    private int pathIndex;
    private Vector3 originalScale;

    private void Awake() {
        originalScale = transform.localScale;
    }

    public void Initialize(BoardPlayer player, int index) {
        boardPlayer = player;
        pathIndex = index;
    }

    private void OnMouseDown() {
        if (boardPlayer != null && boardPlayer.isLocalPlayer) {
            boardPlayer.CmdChooseBranchPath(pathIndex);
        }
    }

    private void OnMouseEnter() {
        transform.localScale = originalScale * 1.2f;
    }

    private void OnMouseExit() {
        transform.localScale = originalScale;
    }
}
