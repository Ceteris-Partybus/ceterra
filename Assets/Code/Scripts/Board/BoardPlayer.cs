using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

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
    [SyncVar(hook = nameof(OnTargetPositionChanged))]
    private Vector3 targetPosition;

    [SyncVar]
    private bool isMoving = false;

    [SyncVar]
    private bool isWaitingForJunctionChoice = false;

    [Header("References")]
    [SerializeField] private Transform junctionArrowPrefab;
    private List<GameObject> junctionArrows = new List<GameObject>();

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10f;
    private SplineKnotIndex nextKnot;

    [Header("Stats")]
    [SyncVar]
    [SerializeField]
    private uint coins;
    public uint Coins => coins;
    public const uint MAX_COINS = 1000;

    [SyncVar]
    [SerializeField]
    private uint health;
    public uint Health => health;
    public const uint MAX_HEALTH = 100;

    protected void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartServer() {
        base.OnStartServer();

        this.coins = 0;
        this.health = MAX_HEALTH;
    }

    public void AddCoins(uint amount) {
        if (coins + amount > MAX_COINS) {
            coins = MAX_COINS;
            uint remaining = coins + amount - MAX_COINS;
            BoardContext.Instance.UpdateFundsStat(remaining);
        }
        else {
            coins += amount;
        }
    }

    public void AddHealth(uint amount) {
        health = (uint)Mathf.Min(health + amount, MAX_HEALTH);
    }

    public void RemoveCoins(uint amount) {
        coins -= amount;
    }

    public void RemoveHealth(uint amount) {
        health -= amount;
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
        Vector3 spawnPosition = BoardContext.Instance.FieldList.Find(splineKnotIndex).Position;
        spawnPosition.y += 1f;
        gameObject.transform.position = spawnPosition;
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

        if (source is MinigameOnePlayer minigamePlayer) {
            AddCoins((uint)Math.Max(0, minigamePlayer.Score));
        }
    }

    // Client-side hook for position updates
    private void OnSplineKnotIndexChanged(SplineKnotIndex oldIndex, SplineKnotIndex newIndex) {
        if (IsActiveForCurrentScene) {
            var field = BoardContext.Instance.FieldList.Find(newIndex);
            StartCoroutine(MoveToFieldCoroutine(field));
        }
    }

    private void OnTargetPositionChanged(Vector3 oldPos, Vector3 newPos) {
        if (IsActiveForCurrentScene && !isServer) {
            StartCoroutine(SmoothMoveToPositionCoroutine(newPos));
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
        StartCoroutine(MoveAlongSplineCoroutine(steps));
    }

    [Server]
    private IEnumerator MoveAlongSplineCoroutine(int steps) {
        var fieldList = BoardContext.Instance.FieldList;
        var remainingSteps = steps;
        while (remainingSteps > 0) {
            var currentField = fieldList.Find(splineKnotIndex);
            var nextFields = currentField.Next;

            var targetField = nextFields.First();
            nextKnot = targetField.SplineKnotIndex;
            if (nextFields.Count > 1) {
                isWaitingForJunctionChoice = true;
                TargetShowJunctionChoice();
                yield return new WaitUntil(() => !isWaitingForJunctionChoice);

                targetField = fieldList.Find(nextKnot);
            }
            yield return StartCoroutine(SmoothMoveToKnot(targetField));

            SplineKnotIndex = nextKnot;
            remainingSteps--;
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        var finalField = fieldList.Find(splineKnotIndex);
        BoardContext.Instance.OnPlayerMovementComplete(this);
        finalField.Invoke(this);
    }

    [Server]
    private IEnumerator SmoothMoveToKnot(Field targetField) {
        var targetPos = targetField.Position;
        targetPos.y += 1f;
        targetPosition = targetPos;

        var duration = Vector3.Distance(transform.position, targetPos) / moveSpeed;
        var elapsed = 0f;
        var startPos = transform.position;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;
    }

    private IEnumerator SmoothMoveToPositionCoroutine(Vector3 targetPos) {
        var startPos = transform.position;
        var duration = Vector3.Distance(startPos, targetPos) / moveSpeed;
        var elapsed = 0f;

        while (elapsed < duration && Vector3.Distance(transform.position, targetPos) > 0.1f) {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    [TargetRpc]
    private void TargetShowJunctionChoice() {
        if (!isLocalPlayer || junctionArrowPrefab == null) { return; }

        var fieldList = BoardContext.Instance.FieldList;
        var currentField = fieldList.Find(splineKnotIndex);
        var nextFields = currentField.Next;

        foreach (var nextField in nextFields) {
            var junctionArrow = Instantiate(junctionArrowPrefab.gameObject);

            var deltaX = (nextField.Position.x - currentField.Position.x) / 5;
            var deltaZ = (nextField.Position.z - currentField.Position.z) / 5;
            junctionArrow.transform.position = new Vector3(currentField.Position.x + deltaX, 0f, currentField.Position.z + deltaZ);

            junctionArrow.transform.LookAt(nextField.Position, transform.up);
            junctionArrows.Add(junctionArrow);
        }
    }

    private void Update() {
        if (!isLocalPlayer || !isWaitingForJunctionChoice) { return; }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            CmdChooseJunctionPath(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            CmdChooseJunctionPath(1);
        }
    }

    [Command]
    public void CmdChooseJunctionPath(int pathIndex) {
        if (!IsActiveForCurrentScene || !isWaitingForJunctionChoice) {
            return;
        }

        var fieldList = BoardContext.Instance.FieldList;
        var currentField = fieldList.Find(splineKnotIndex);
        var nextFields = currentField.Next;

        if (pathIndex < 0 || pathIndex >= nextFields.Count) {
            return;
        }

        var chosenField = nextFields[pathIndex];

        nextKnot = chosenField.SplineKnotIndex;
        isWaitingForJunctionChoice = false;
        TargetHideJunctionChoice();
    }

    [TargetRpc]
    private void TargetHideJunctionChoice() {
        foreach (var arrow in junctionArrows) {
            if (arrow != null) {
                Destroy(arrow);
            }
        }
        junctionArrows.Clear();
    }

    private IEnumerator MoveToFieldCoroutine(Field targetField) {
        var startPos = transform.position;
        var targetPos = targetField.Position;
        targetPos.y += gameObject.transform.localScale.y / 2f;

        var duration = Vector3.Distance(startPos, targetPos) / moveSpeed;
        var elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }
}