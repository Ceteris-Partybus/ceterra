using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BoardPlayer : SceneConditionalPlayer {
    [Header("Position")]
    [SyncVar(hook = nameof(OnSplineKnotIndexChanged))]
    private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex {
        get => splineKnotIndex;
        [Server]
        set => splineKnotIndex = value;
    }

    [Header("Movement")]
    [SyncVar]
    private bool isMoving = false;
    public bool IsMoving => isMoving;

    public static readonly float moveSpeed = 5f;
    public static readonly AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == (NetworkManager.singleton as NetworkRoomManager)?.GameplayScene;
    }

    [Server]
    protected override void OnServerInitialize() {
        Debug.Log($"[Server] BoardPlayer {name} initialized for board scene");
        // Initialize board-specific state
        isMoving = false;
        // Set spawn position, etc.
    }

    [Server]
    protected override void OnServerCleanup() {
        Debug.Log($"[Server] BoardPlayer {name} cleaned up");
        // Stop any ongoing movement
        StopAllCoroutines();
        isMoving = false;
    }

    [Server]
    protected override void OnServerReceiveData(SceneConditionalPlayer source) {
        // Receive data from minigame players or other sources
        Debug.Log($"[Server] BoardPlayer received data from {source.GetType().Name}");
    }

    // Client-side hook for position updates
    private void OnSplineKnotIndexChanged(SplineKnotIndex oldIndex, SplineKnotIndex newIndex) {
        if (IsActiveForCurrentScene) {
            Field field = BoardContext.Instance.FieldList.Find(newIndex);
            StartCoroutine(MoveToFieldCoroutine(field));
        }
    }

    [Command]
    public void CmdRollDice() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving) {
            return;
        }

        int diceValue = Random.Range(1, 7);
        BoardContext.Instance.ProcessDiceRoll(this, diceValue);
    }

    [Server]
    public void MoveToField(int steps) {
        if (!IsActiveForCurrentScene || isMoving) {
            return;
        }

        isMoving = true;
        StartCoroutine(MoveStepByStepCoroutine(steps));
    }

    [Server]
    private IEnumerator MoveStepByStepCoroutine(int steps) {
        FieldList fieldList = BoardContext.Instance.FieldList;
        Field currentField = fieldList.Find(splineKnotIndex);

        for (int step = 0; step < steps; step++) {
            currentField = fieldList.Find(splineKnotIndex).Next[0];
            SplineKnotIndex = currentField.SplineKnotIndex;
            yield return new WaitForSeconds(0.8f); // Wait for movement animation
        }

        isMoving = false;
        BoardContext.Instance.OnPlayerMovementComplete(this);
        currentField.Invoke(this);
    }

    private IEnumerator MoveToFieldCoroutine(Field targetField) {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetField.Position;
        targetPos.y += gameObject.transform.localScale.y / 2f;

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
}