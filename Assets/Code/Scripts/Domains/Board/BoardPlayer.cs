using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
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
    private bool isMoving = false;
    public bool IsMoving {
        get => isMoving;
        set { isMoving = value; }
    }
    [SyncVar]
    private bool isJumping = false;
    public bool IsJumping {
        get => isJumping;
        set { isJumping = value; }
    }

    [SerializeField] private float moveSpeed;
    [SerializeField] private float movementLerp;
    [SerializeField] private float rotationLerp;

    [SyncVar]
    private float normalizedSplinePosition;
    public float NormalizedSplinePosition {
        get => normalizedSplinePosition;
        set { normalizedSplinePosition = value; }
    }

    [Header("Stats")]
    [SyncVar(hook = nameof(OnCoinsChanged))]
    [SerializeField]
    private int coins;
    public int Coins => coins;
    public const int MAX_COINS = 1000;

    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField]
    private int health;
    public int Health => health;
    public const int MAX_HEALTH = 100;
    private BoardPlayerVisualHandler visualHandler;
    private float zoomBlendTime = 0.5f;

    [SyncVar]
    private bool animationFinished = true;
    public bool IsAnimationFinished {
        get => animationFinished;
        set { animationFinished = value; }
    }

    private Character character;
    private Dice dice;

    protected void Start() {
        DontDestroyOnLoad(gameObject);
        splineContainer = FindFirstObjectByType<SplineContainer>();
    }

    public override void OnStartServer() {
        base.OnStartServer();

        this.coins = 50;
        this.health = MAX_HEALTH;
    }

    [Server]
    public void ServerTransferCharacterSelection(LobbyPlayer lobbyPlayer) {
        TransferCharacterSelection(lobbyPlayer);

        var boardPlayers = GameManager.Singleton.roomSlots.Count;
        StartCoroutine(DelayedRpcTransfer());

        IEnumerator DelayedRpcTransfer() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == boardPlayers);
            RpcTransferCharacterSelection(lobbyPlayer);
        }
    }

    private void TransferCharacterSelection(LobbyPlayer lobbyPlayer) {
        lobbyPlayer.StopFacingCameraCoroutine();

        this.character = lobbyPlayer.CurrentCharacterInstance.GetComponent<Character>();
        this.character.transform.SetParent(transform, false);
        this.dice = lobbyPlayer.CurrentDiceInstance.GetComponent<Dice>();
        visualHandler = GetComponent<BoardPlayerVisualHandler>().Initialize(this.character, this.dice);
    }

    [ClientRpc]
    private void RpcTransferCharacterSelection(LobbyPlayer lobbyPlayer) {
        TransferCharacterSelection(lobbyPlayer);
    }

    [Command]
    public void CmdClaimQuizReward(int amount) {
        AddCoins(amount);
    }

    [Server]
    public void AddCoins(int amount) {
        if (coins + amount > MAX_COINS) {
            coins = MAX_COINS;
            int remaining = coins + amount - MAX_COINS;
            BoardContext.Instance.UpdateFundsStat(remaining);
        }
        else {
            coins += amount;
        }
        RpcTriggerBlockingAnimation(AnimationType.COIN_GAIN);
    }

    public void AddHealth(int amount) {
        health = Math.Min(health + amount, MAX_HEALTH);
        RpcTriggerBlockingAnimation(AnimationType.HEALTH_GAIN);
    }

    public void RemoveCoins(int amount) {
        coins = Math.Max(0, coins - amount);
        RpcTriggerBlockingAnimation(AnimationType.COIN_LOSS);
    }

    public void RemoveHealth(int amount) {
        health = Math.Max(0, health - amount);
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

    [Command]
    private void CmdAnimationComplete() {
        IsAnimationFinished = true;
    }

    [ClientRpc]
    public void RpcTriggerAnimation(AnimationType animationType) {
        visualHandler.TriggerAnimation(animationType);
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == (NetworkManager.singleton as NetworkRoomManager)?.GameplayScene;
    }

    [Server]
    protected override void OnServerInitialize() {
        StartCoroutine(WaitForFieldInitialization());

        IEnumerator WaitForFieldInitialization() {
            yield return new WaitUntil(() => BoardContext.Instance != null && BoardContext.Instance.FieldBehaviourList != null);
            isMoving = false;
            var spawnPosition = GameManager.Singleton.GetStartPosition();
            gameObject.transform.position = spawnPosition.position;
        }
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
            AddCoins(Math.Max(0, minigamePlayer.Score));
            RemoveHealth(minigamePlayer.Score);
        }
        else if (source is MgGarbagePlayer garbagePlayer) {
            AddCoins(Math.Max(0, garbagePlayer.Score));
        }
        else if (source is MgQuizduelPlayer quizDuelPlayer) {
            AddCoins(Math.Max(0, quizDuelPlayer.EarnedCoinReward));
        }
        else if (source is MgOceanPlayer oceanPlayer) {
            AddCoins(Math.Max(0, oceanPlayer.Score));
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

    private void OnCoinsChanged(int old, int new_) {
        if (isLocalPlayer) {
            BoardOverlay.Instance.UpdateLocalPlayerCoins(new_);
        }
        else {
            BoardOverlay.Instance.UpdateRemotePlayerCoins(new_, PlayerId);
        }
    }

    private void OnHealthChanged(int old, int new_) {
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
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving || dice.IsSpinning) {
            return;
        }
        RpcStartDiceRoll();
    }

    [Command]
    public void CmdToggleBoardOverview() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving || dice.IsSpinning) {
            return;
        }
        CameraHandler.Instance.RpcToggleBoardOverview();
    }

    [Command]
    public void CmdEndRollDice() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving) {
            return;
        }

        var diceValue = dice.RandomValue;
        RpcStartRollSequence(diceValue);
        StartCoroutine(StartRollSequence(diceValue));
    }

    [Command]
    public void CmdRollDiceCancel() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || isMoving) {
            return;
        }
        RpcEndDiceCancel();
    }

    private IEnumerator StartRollSequence(int diceValue) {
        yield return visualHandler.StartRollSequence(diceValue);
        if (isServer) { BoardContext.Instance.ProcessDiceRoll(this, diceValue); }
    }

    [ClientRpc]
    private void RpcStartRollSequence(int diceValue) {
        StartCoroutine(StartRollSequence(diceValue));
    }

    [ClientRpc]
    private void RpcEndDiceCancel() {
        CameraHandler.Instance.ZoomOut();
        visualHandler.OnRollCancel();
    }

    [ClientRpc]
    private void RpcStartDiceRoll() {
        visualHandler.OnRollStart();
    }

    [ClientRpc]
    private void RpcUpdateDiceResultLabel(string value) {
        visualHandler.DiceResultLabel(value);
    }

    [ClientRpc]
    private void RpcHideDiceResultLabel() {
        visualHandler.HideDiceResultLabel();
    }

    [Server]
    public void MoveToField(int steps) {
        if (!IsActiveForCurrentScene || isMoving) { return; }

        StartCoroutine(MoveAlongSplineCoroutine(steps));
    }

    [Server]
    private IEnumerator MoveAlongSplineCoroutine(int steps) {
        var fieldBehaviourList = BoardContext.Instance.FieldBehaviourList;
        var remainingSteps = steps;

        while (remainingSteps > 0) {
            RpcUpdateDiceResultLabel(remainingSteps.ToString());
            var targetField = fieldBehaviourList.Find(splineKnotIndex).GetNextTargetField();
            yield return StartCoroutine(ServerSmoothMoveToKnot(targetField));

            SplineKnotIndex = targetField.SplineKnotIndex;
            if (!targetField.SkipStepCount) { remainingSteps--; }

            if (remainingSteps > 0) {
                if (targetField.PausesMovement) {
                    yield return StartCoroutine(EnsureTargetPosition(targetField.Position));
                }
                yield return StartCoroutine(targetField.InvokeOnPlayerCross(this));
            }
        }
        yield return StartCoroutine(EnsureTargetPosition(fieldBehaviourList.Find(splineKnotIndex).Position));

        RpcTriggerAnimation(AnimationType.IDLE);
        isMoving = false;
        RpcHideDiceResultLabel();

        CameraHandler.Instance.RpcZoomIn();
        yield return new WaitForSeconds(zoomBlendTime);

        var finalField = fieldBehaviourList.Find(splineKnotIndex);
        yield return StartCoroutine(finalField.InvokeOnPlayerLand(this));

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(zoomBlendTime);

        BoardContext.Instance.OnPlayerMovementComplete(this);
    }

    [Server]
    private IEnumerator EnsureTargetPosition(Vector3 targetPosition) {
        while (Vector3.Distance(transform.position, targetPosition) > .05f) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * .5f * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    [Server]
    private IEnumerator ServerSmoothMoveToKnot(FieldBehaviour targetField) {
        var currentSpline = splineContainer.Splines[splineKnotIndex.Spline];
        var targetSpline = splineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var spline = targetSpline;
        var normalizedTargetPosition = targetField.NormalizedSplinePosition;

        if (!IsMoving) {
            isJumping = false;
            isMoving = true;
            RpcTriggerAnimation(AnimationType.RUN);
        }

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

        while (normalizedSplinePosition != normalizedTargetPosition) {
            normalizedSplinePosition = Mathf.MoveTowards(normalizedSplinePosition, normalizedTargetPosition, moveSpeed / spline.GetLength() * Time.deltaTime);
            yield return null;
        }

        normalizedSplinePosition = targetField.NormalizedSplinePosition;
    }

    void MoveAndRotate() {
        var movementBlend = Mathf.Pow(0.5f, Time.deltaTime * movementLerp);
        var targetPosition = splineContainer.EvaluatePosition(splineKnotIndex.Spline, normalizedSplinePosition);
        transform.position = Vector3.Lerp(targetPosition, transform.position, movementBlend);

        if (isMoving) {
            splineContainer.Splines[splineKnotIndex.Spline].Evaluate(normalizedSplinePosition, out float3 _, out float3 direction, out float3 _);
            var worldDirection = splineContainer.transform.TransformDirection(direction);

            if (worldDirection.sqrMagnitude > 0.0001f) {
                visualHandler.SetMovementRotation(Quaternion.LookRotation(worldDirection, Vector3.up), rotationLerp);
            }
        }
    }

    void Update() {
        if (isJumping) { return; }
        if (isMoving) { MoveAndRotate(); return; }
        if (isLocalPlayer) { HandleInput(); }

        visualHandler?.MakeCharacterFaceCamera();
    }

    private void HandleInput() {
        var pressedSpaceToEndRoll = Input.GetKeyDown(KeyCode.Space) && dice.IsSpinning;
        if (pressedSpaceToEndRoll) { CmdEndRollDice(); return; }

        var pressedEscapeToCancelRoll = Input.GetKeyDown(KeyCode.Escape) && dice.IsSpinning;
        if (pressedEscapeToCancelRoll) { CmdRollDiceCancel(); return; }

        var pressedEscapeToCancelBoardOverview = Input.GetKeyDown(KeyCode.Escape) && CameraHandler.Instance.IsShowingBoard;
        if (pressedEscapeToCancelBoardOverview) { CmdToggleBoardOverview(); return; }
    }
}