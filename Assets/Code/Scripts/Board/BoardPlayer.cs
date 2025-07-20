using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.Splines;

public class BoardPlayer : NetworkBehaviour {
    [Header("Player Settings")]
    public TextMesh playerNameText;
    public GameObject floatingInfo;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SyncVar]
    public string playerName;

    [SyncVar(hook = nameof(OnFieldChanged))]
    public SplineKnotIndex currentSplineKnotIndex;

    public SplineKnotIndex CurrentSplineKnotIndex {
        get;
        set;
    }

    [SyncVar]
    public bool isMoving = false;

    void OnFieldChanged(SplineKnotIndex oldIndex, SplineKnotIndex newIndex) {
        if (!isServer && !oldIndex.Equals(newIndex)) {
            StartCoroutine(MoveToFieldCoroutine(GameManager.Instance.fieldList.Find(newIndex)));
        }
    }

    public override void OnStartLocalPlayer() {
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 2, -10);

        floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
        floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);
        CmdSetupPlayer(name);
    }

    public override void OnStartServer() {
        if (GameManager.Instance != null) {
            GameManager.Instance.RegisterPlayer(this);
        }
    }

    public override void OnStopServer() {
        if (GameManager.Instance != null) {
            GameManager.Instance.UnregisterPlayer(this);
        }
    }

    [Command]
    public void CmdSetupPlayer(string _name) {
        playerName = _name;
    }

    [Command]
    public void CmdRollDice() {
        if (GameManager.Instance == null) { return; }
        if (!GameManager.Instance.IsPlayerTurn(this)) {
            return;
        }
        if (isMoving) { return; }

        int diceValue = Random.Range(1, 7);

        GameManager.Instance.ProcessDiceRoll(this, diceValue);
    }

    [Server]
    public void MoveToField(int steps) {
        if (GameManager.Instance == null) { return; }
        if (isMoving) { return; }

        isMoving = true;
        StartCoroutine(MoveStepByStepCoroutine(steps));
    }

    IEnumerator MoveStepByStepCoroutine(int steps) {
        for (int step = 0; step < steps; step++) {
            var currentField = GameManager.Instance.fieldList.Find(currentSplineKnotIndex).Next[0];
            currentSplineKnotIndex = currentField.SplineKnotIndex;
            yield return StartCoroutine(MoveToFieldCoroutine(currentField)); // TODO: Implement junction handling
            yield return new WaitForSeconds(0.2f);
        }

        isMoving = false;
    }

    IEnumerator MoveToFieldCoroutine(Field targetField) {
        if (GameManager.Instance == null) { yield break; }

        Vector3 startPos = transform.position;
        Vector3 targetPos = targetField.Position;

        float duration = Vector3.Distance(startPos, targetPos) / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveT = moveCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPos, curveT);
            yield return null;
        }

        transform.position = targetPos;
    }

    void Update() {
        if (!isLocalPlayer) {
            floatingInfo.transform.LookAt(Camera.main.transform);
            return;
        }
    }
}