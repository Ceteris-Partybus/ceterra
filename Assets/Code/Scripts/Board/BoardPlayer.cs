using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;
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
    [SyncVar]
    private bool isMoving = false;

    [SyncVar]
    private bool isWaitingForBranchChoice = false;

    [Header("References")]
    [SerializeField] private Transform branchArrowPrefab;
    private List<GameObject> branchArrows = new List<GameObject>();

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10f;
    private SplineKnotIndex nextKnot;

    [Header("Stats")]
    [SyncVar(hook = nameof(OnCoinsChanged))]
    [SerializeField]
    private uint coins;
    public uint Coins => coins;
    public const uint MAX_COINS = 1000;

    [SyncVar(hook = nameof(OnHealthChanged))]
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

    [Command]
    public void CmdClaimQuizReward(uint amount) {
        AddCoins(amount);
    }

    [Server]
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
        Vector3 spawnPosition = BoardContext.Instance.FieldBehaviourList.Find(splineKnotIndex).Position;
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
            RemoveHealth((uint)minigamePlayer.Score);
        }
        else if (source is MgGarbagePlayer garbagePlayer) {
            AddCoins((uint)Math.Max(0, garbagePlayer.Score));
        }
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive) {
            if (!BoardOverlay.Instance.IsPlayerAdded(PlayerId)) {
                BoardOverlay.Instance.AddPlayer(this);
            }
            BoardOverlay.Instance.UpdateRemotePlayerHealth(health, PlayerId);
            BoardOverlay.Instance.UpdateRemotePlayerCoins(coins, PlayerId);
        }
        else if (isLocalPlayer && isActive) {
            BoardOverlay.Instance.UpdateLocalPlayerHealth(health);
            BoardOverlay.Instance.UpdateLocalPlayerCoins(coins);
        }
    }

    private void OnCoinsChanged(uint old, uint new_) {
        if (isLocalPlayer) {
            BoardOverlay.Instance.UpdateLocalPlayerCoins(new_);
        }
        else {
            BoardOverlay.Instance.UpdateRemotePlayerCoins(new_, PlayerId);
        }
    }

    private void OnHealthChanged(uint old, uint new_) {
        if (isLocalPlayer) {
            BoardOverlay.Instance.UpdateLocalPlayerHealth(new_);
        }
        else {
            BoardOverlay.Instance.UpdateRemotePlayerHealth(new_, PlayerId);
        }
    }

    public override void OnStopClient() {
        base.OnStopClient();
        BoardOverlay.Instance.RemovePlayer(this);
    }

    // Client-side hook for position updates
    private void OnSplineKnotIndexChanged(SplineKnotIndex oldIndex, SplineKnotIndex newIndex) {
        if (IsActiveForCurrentScene) {
            var fieldBehaviour = BoardContext.Instance.FieldBehaviourList.Find(newIndex);
            StartCoroutine(SmoothMoveToKnot(fieldBehaviour));
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
        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var remainingSteps = steps;
        while (remainingSteps > 0) {
            var currentField = fieldBehaviourList.Find(splineKnotIndex);
            var nextFields = currentField.Next;

            var targetField = nextFields.First();
            nextKnot = targetField.SplineKnotIndex;
            if (nextFields.Count > 1) {
                isWaitingForBranchChoice = true;
                TargetShowBranchArrows();
                yield return new WaitUntil(() => !isWaitingForBranchChoice);

                targetField = fieldBehaviourList.Find(nextKnot);
            }
            SplineKnotIndex = nextKnot;
            yield return StartCoroutine(SmoothMoveToKnot(targetField));

            remainingSteps--;
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        var finalField = fieldBehaviourList.Find(splineKnotIndex);

        yield return StartCoroutine(finalField.InvokeFieldAsync(this));

        BoardContext.Instance.OnPlayerMovementComplete(this);
    }

    private IEnumerator SmoothMoveToKnot(FieldBehaviour targetField) {
        var startPos = transform.position;
        var targetPos = targetField.Position;
        targetPos.y = transform.position.y;

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

    [TargetRpc]
    private void TargetShowBranchArrows() {
        if (!isLocalPlayer || branchArrowPrefab == null) { return; }

        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var currentField = fieldBehaviourList.Find(splineKnotIndex);
        var nextFields = currentField.Next;

        for (var i = 0; i < nextFields.Count; i++) {
            var branchArrow = Instantiate(branchArrowPrefab.gameObject);

            var deltaX = (nextFields[i].Position.x - currentField.Position.x) / 2;
            var deltaZ = (nextFields[i].Position.z - currentField.Position.z) / 2;
            branchArrow.transform.position = new Vector3(currentField.Position.x + deltaX, 0f, currentField.Position.z + deltaZ);
            branchArrow.transform.LookAt(nextFields[i].Position, transform.up);

            branchArrow.GetComponent<BranchArrowMouseEventHandler>()?.Initialize(this, i);
            branchArrows.Add(branchArrow);
        }
    }

    [TargetRpc]
    private void TargetHideBranchArrows() {
        foreach (var arrow in branchArrows) {
            Destroy(arrow);
        }
        branchArrows.Clear();
    }

    [Command]
    public void CmdChooseBranchPath(int pathIndex) {
        if (!IsActiveForCurrentScene || !isWaitingForBranchChoice) {
            return;
        }

        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var currentField = fieldBehaviourList.Find(splineKnotIndex);
        var nextFields = currentField.Next;

        if (pathIndex < 0 || pathIndex >= nextFields.Count) {
            return;
        }

        var chosenField = nextFields[pathIndex];

        nextKnot = chosenField.SplineKnotIndex;
        isWaitingForBranchChoice = false;
        TargetHideBranchArrows();
    }
}