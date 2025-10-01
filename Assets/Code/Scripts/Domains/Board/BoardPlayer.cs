using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class BoardPlayer : SceneConditionalPlayer {
    [Header("Position")]
    [SyncVar]
    private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex {
        get => splineKnotIndex;
        [Server]
        set => splineKnotIndex = value;
    }

    private SplineContainer splineContainer;
    public SplineContainer SplineContainer => splineContainer;

    [Header("Movement")]
    [SyncVar]
    private bool isRolling = false;

    [SyncVar]
    private bool isMoving = false;
    private bool IsMoving {
        get => isMoving;
        set { isMoving = value; }
    }

    [SerializeField] private float moveSpeed;
    [SerializeField] private float movementLerp;
    [SerializeField] private float rotationLerp;
    [SerializeField] private Transform playerModel;

    [SyncVar(hook = nameof(OnNormalizedSplinePositionChanged))]
    private float normalizedSplinePosition;
    public float NormalizedSplinePosition => normalizedSplinePosition;

    [SyncVar]
    private bool isWaitingForBranchChoice = false;

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
    private BoardPlayerVisualHandler visualHandler;
    private float zoomBlendTime = 0.5f;

    [SyncVar]
    private bool animationFinished = true;
    public bool IsAnimationFinished {
        get => animationFinished;
        set { animationFinished = value; }
    }

    protected void Start() {
        DontDestroyOnLoad(gameObject);
        splineContainer = FindFirstObjectByType<SplineContainer>();
        visualHandler = GetComponentInChildren<BoardPlayerVisualHandler>();
    }

    public override void OnStartServer() {
        base.OnStartServer();

        this.coins = 50;
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
            int remaining = (int)(coins + amount - MAX_COINS);
            BoardContext.Instance.UpdateFundsStat(remaining);
        }
        else {
            coins += amount;
        }
        RpcTriggerBlockingAnimation(AnimationType.COIN_GAIN);
    }

    public void AddHealth(uint amount) {
        health = (uint)Mathf.Min(health + amount, MAX_HEALTH);
        RpcTriggerBlockingAnimation(AnimationType.HEALTH_GAIN);
    }

    public void RemoveCoins(uint amount) {
        coins -= amount;
        RpcTriggerBlockingAnimation(AnimationType.COIN_LOSS);
    }

    public void RemoveHealth(uint amount) {
        health -= amount;
        RpcTriggerBlockingAnimation(AnimationType.HEALTH_LOSS);
    }

    [ClientRpc]
    private void RpcTriggerBlockingAnimation(AnimationType animationType) {
        IsAnimationFinished = false;
        StartCoroutine(TriggerAnimationCoroutine());

        IEnumerator TriggerAnimationCoroutine() {
            var waitWhile = visualHandler.TriggerBlockingAnimation(animationType);
            yield return null;
            yield return waitWhile;

            if (isLocalPlayer) { CmdAnimationComplete(); }
        }
    }

    [ClientRpc]
    private void RpcTriggerAnimation(AnimationType animationType) {
        visualHandler.TriggerAnimation(animationType);
    }

    [Command]
    private void CmdAnimationComplete() {
        IsAnimationFinished = true;
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == (NetworkManager.singleton as NetworkRoomManager)?.GameplayScene;
    }

    [Server]
    protected override void OnServerInitialize() {
        Debug.Log($"[Server] BoardPlayer {name} initialized for board scene");
        // Initialize board-specific state
        IsMoving = false;
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
        IsMoving = false;
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
        else if (source is MgQuizduelPlayer quizDuelPlayer) {
            AddCoins((uint)Math.Max(0, quizDuelPlayer.EarnedCoinReward));
        } else if (source is MgOceanPlayer oceanPlayer){
            AddCoins((uint)Math.Max(0, oceanPlayer.Score));
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
            BoardOverlay.Instance.UpdateLocalPlayerName(PlayerName);
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

    [Command]
    public void CmdRollDice() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving || isRolling) {
            return;
        }
        isRolling = true;
        RpcStartDiceRoll();
    }

    [Command]
    public void CmdToggleBoardOverview() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving || isRolling) {
            return;
        }
        RpcToggleBoardOverview();
    }

    [Command]
    public void CmdEndRollDice() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving) {
            return;
        }

        var diceValue = Random.Range(1, 11);
        StartCoroutine(StartRollSequence(diceValue));
    }

    [Command]
    public void CmdRollDiceCancel() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving) {
            return;
        }

        isRolling = false;
        RpcEndDiceCancel();
    }

    [Server]
    private IEnumerator StartRollSequence(int diceValue) {
        RpcOnRollJump();
        yield return new WaitForSeconds(0.09f);

        RpcShowDiceResultLabel(diceValue);
        yield return new WaitForSeconds(0.5f);

        isRolling = false;
        RpcEndDiceRoll(diceValue);
        yield return new WaitForSeconds(0.6f);

        BoardContext.Instance.ProcessDiceRoll(this, diceValue);
    }

    [ClientRpc]
    private void RpcToggleBoardOverview() {
        CameraHandler.Instance.ToggleBoardOverview();
    }

    [ClientRpc]
    private void RpcOnRollJump() {
        visualHandler.OnRollJump();
    }

    [ClientRpc]
    private void RpcEndDiceCancel() {
        CameraHandler.Instance.ZoomOut();
        visualHandler.OnRollCancel();
    }

    [ClientRpc]
    private void RpcStartDiceRoll() {
        CameraHandler.Instance.ZoomIn();
        visualHandler.OnRollStart();
    }

    [ClientRpc]
    private void RpcEndDiceRoll(int roll) {
        CameraHandler.Instance.ZoomOut();
        visualHandler.OnRollEnd(roll);
    }

    [ClientRpc]
    private void RpcShowDiceResultLabel(int steps) {
        visualHandler.OnRollDisplay(steps);
    }

    [ClientRpc]
    private void RpcUpdateDiceResultLabel(string value) {
        visualHandler.DiceResultLabel = value;
    }

    [ClientRpc]
    private void RpcHideDiceResultLabel() {
        visualHandler.HideDiceResultLabel();
    }

    [Server]
    public void MoveToField(int steps) {
        if (!IsActiveForCurrentScene || IsMoving) { return; }

        StartCoroutine(MoveAlongSplineCoroutine(steps));
    }

    [Server]
    private IEnumerator MoveAlongSplineCoroutine(int steps) {
        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var remainingSteps = steps;

        IsMoving = true;

        RpcTriggerAnimation(AnimationType.RUN);
        while (remainingSteps > 0) {
            RpcUpdateDiceResultLabel(remainingSteps.ToString());
            var nextFields = fieldBehaviourList.Find(splineKnotIndex).Next;

            var targetField = nextFields.First();
            nextKnot = targetField.SplineKnotIndex;
            if (nextFields.Count > 1) {
                IsMoving = false;
                RpcTriggerAnimation(AnimationType.THINK);
                isWaitingForBranchChoice = true;
                TargetShowBranchArrows();
                yield return new WaitUntil(() => !isWaitingForBranchChoice);

                targetField = fieldBehaviourList.Find(nextKnot);
                RpcTriggerAnimation(AnimationType.RUN);
                IsMoving = true;
            }
            yield return StartCoroutine(ServerSmoothMoveToKnot(targetField));
            SplineKnotIndex = nextKnot;
            remainingSteps--;
            yield return new WaitForSeconds(0.15f);
        }

        RpcTriggerAnimation(AnimationType.IDLE);
        IsMoving = false;
        RpcHideDiceResultLabel();

        CameraHandler.Instance.RpcZoomIn();
        yield return new WaitForSeconds(zoomBlendTime);

        var finalField = fieldBehaviourList.Find(splineKnotIndex);
        yield return StartCoroutine(finalField.InvokeFieldAsync(this));

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(zoomBlendTime);

        BoardContext.Instance.OnPlayerMovementComplete(this);
    }

    [Server]
    private IEnumerator ServerSmoothMoveToKnot(FieldBehaviour targetField) {
        var currentSpline = splineContainer.Splines[splineKnotIndex.Spline];
        var targetSpline = splineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var spline = targetSpline;
        var normalizedTargetPosition = targetField.NormalizedSplinePosition;

        if (SplineKnotIndex.Spline != targetField.SplineKnotIndex.Spline) {
            if (targetField.SplineKnotIndex.Knot == 1) {
                SplineKnotIndex = targetField.SplineKnotIndex;
                normalizedSplinePosition = 0f;
            }
            else {
                normalizedTargetPosition = 1f;
                spline = currentSpline;
            }
        }

        if (targetField.SplineKnotIndex.Knot == 0 && targetSpline.Closed) {
            normalizedTargetPosition = 1f;
        }

        while (Mathf.Abs(normalizedSplinePosition - normalizedTargetPosition) > 0.001f) {
            normalizedSplinePosition = Mathf.MoveTowards(normalizedSplinePosition, normalizedTargetPosition, moveSpeed / spline.GetLength() * Time.deltaTime);
            yield return null;
        }

        normalizedSplinePosition = targetField.NormalizedSplinePosition;
    }

    private void OnNormalizedSplinePositionChanged(float _, float newValue) {
        if (!isServer && IsActiveForCurrentScene) {
            normalizedSplinePosition = newValue;
        }
    }

    void MoveAndRotate() {
        var movementBlend = Mathf.Pow(0.5f, Time.deltaTime * movementLerp);
        var targetPosition = splineContainer.EvaluatePosition(splineKnotIndex.Spline, normalizedSplinePosition);

        transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - movementBlend);

        if (IsMoving) {
            splineContainer.Splines[splineKnotIndex.Spline].Evaluate(normalizedSplinePosition, out float3 _, out float3 direction, out float3 _);
            var worldDirection = splineContainer.transform.TransformDirection(direction);

            if (worldDirection.sqrMagnitude > 0.0001f) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(worldDirection, Vector3.up), rotationLerp * Time.deltaTime);
            }
        }
    }

    [TargetRpc]
    private void TargetShowBranchArrows() {
        if (!isLocalPlayer) { return; }

        var nextFields = BoardContext.Instance.FieldBehaviourList.Find(splineKnotIndex).Next;

        visualHandler.ShowBranchArrows(nextFields, this);
    }

    [TargetRpc]
    private void TargetHideBranchArrows() {
        visualHandler.HideBranchArrows();
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

    void Update() {
        if (IsMoving) {
            MoveAndRotate();
            return;
        }

        FaceCamera();

        if (isLocalPlayer) {
            if (Input.GetKeyDown(KeyCode.Space) && visualHandler.IsDiceSpinning) {
                CmdEndRollDice();
            }
            else if (Input.GetKeyDown(KeyCode.Escape)) {
                if (visualHandler.IsDiceSpinning) {
                    CmdRollDiceCancel();
                }
                else if (CameraHandler.Instance.IsShowingBoard) {
                    CmdToggleBoardOverview();
                }
            }
        }
    }

    private void FaceCamera() {
        var directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0;
        if (directionToCamera.sqrMagnitude > 0.0001f) {
            var targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerp * Time.deltaTime);
        }
        visualHandler.CleanRotation();
    }
}