using UnityEngine;

public class BranchArrowMouseEventHandler : MonoBehaviour {
    private JunctionFieldBehaviour junctionField;
    private int pathIndex;
    private Vector3 originalScale;

    private void Awake() {
        originalScale = transform.localScale;
    }

    public void Initialize(JunctionFieldBehaviour junctionField, int pathIndex) {
        this.junctionField = junctionField;
        this.pathIndex = pathIndex;
    }

    private void OnMouseDown() {
        junctionField.CmdChooseBranchPath(pathIndex);
    }

    private void OnMouseEnter() {
        transform.localScale = originalScale * 1.2f;
    }

    private void OnMouseExit() {
        transform.localScale = originalScale;
    }
}
