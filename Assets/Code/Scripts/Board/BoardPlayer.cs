using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BoardPlayer : SceneConditionalPlayer {
    [Header("Position")]
    [SerializeField]
    [SyncVar(hook = nameof(OnSplineKnotIndexChanged))]
    private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex {
        get => splineKnotIndex;
        set => splineKnotIndex = value;
    }

    [Header("Movement")]
    [SyncVar]
    [SerializeField]
    private bool isMoving = false;
    public bool IsMoving {
        get => isMoving;
        set => isMoving = value;
    }

    public static readonly float moveSpeed = 5f;
    public static readonly AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected override void Start() {
        base.Start();
        DontDestroyOnLoad(gameObject);
    }

    public void OnSplineKnotIndexChanged(SplineKnotIndex oldIndex, SplineKnotIndex newIndex) {
        Field field = BoardContext.Instance.FieldList.Find(newIndex);
        StartCoroutine(MoveToFieldCoroutine(field));
    }

    [Server]
    public void MoveToField(int steps) {
        if (isMoving) {
            return;
        }

        isMoving = true;
        StartCoroutine(MoveStepByStepCoroutine(steps));
    }

    [Command]
    public void CmdRollDice() {
        if (!BoardContext.Instance.IsPlayerTurn(this)) {
            return;
        }
        if (isMoving) { return; }

        int diceValue = Random.Range(1, 7);

        BoardContext.Instance.ProcessDiceRoll(this, diceValue);
    }

    private IEnumerator MoveStepByStepCoroutine(int steps) {
        FieldList fieldList = BoardContext.Instance.FieldList;
        Field currentField = fieldList.Find(splineKnotIndex);

        for (int step = 0; step < steps; step++) {
            currentField = fieldList.Find(splineKnotIndex).Next[0]; // TODO: Junctions
            splineKnotIndex = currentField.SplineKnotIndex;
            yield return StartCoroutine(MoveToFieldCoroutine(currentField));
            yield return new WaitForSeconds(0.2f);
        }

        isMoving = false;
        BoardContext.Instance.OnPlayerMovementComplete(this);

        currentField.Invoke(this);
    }

    private IEnumerator MoveToFieldCoroutine(Field targetField) {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetField.Position;
        targetPos.y += gameObject.transform.localScale.y / 2f; // Adjust for player height

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

    public override bool IsEnabledForScene(string sceneName) {
        return sceneName == (NetworkManager.singleton as NetworkRoomManager).GameplayScene;
    }

    protected override void OnTransferDataTo(SceneConditionalPlayer enabledScript) {
        // if (enabledScript is MinigameOnePlayer minigameOnePlayer) {
        //     minigameOnePlayer.SetPlayerData(this);
        // }
    }

    protected override void Cleanup() {
    }

    protected override void Initialize() {
        // move player back to current position ...
    }
}