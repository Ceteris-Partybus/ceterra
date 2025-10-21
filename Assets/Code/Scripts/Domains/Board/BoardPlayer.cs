using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class BoardPlayer : SceneConditionalPlayer {
    [Header("Stats")]
    [SerializeField]
    private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    [Header("Position")]
    [SyncVar]
    private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex {
        get => splineKnotIndex;
        [Server]
        set => splineKnotIndex = value;
    }

    public bool IsMoving => playerMovement.IsMoving;
    public bool IsJumping => playerMovement.IsJumping;

    private SplineContainer splineContainer;
    public SplineContainer SplineContainer => splineContainer;

    [Header("Movement")]
    [SerializeField]
    private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement => playerMovement;

    [SyncVar]
    private bool isFirstLoad = true;
    public bool IsFirstLoad => isFirstLoad;

    [SyncVar]
    private float normalizedSplinePosition;
    public float NormalizedSplinePosition {
        get => normalizedSplinePosition;
        set { normalizedSplinePosition = value; }
    }

    private BoardPlayerVisualHandler visualHandler;
    public BoardPlayerVisualHandler VisualHandler => visualHandler;

    [SyncVar]
    private bool animationFinished = true;
    public bool IsAnimationFinished {
        get => animationFinished;
        set { animationFinished = value; }
    }

    [SyncVar]
    private bool rollSequenceFinished = true;

    private Character character;
    private Dice dice;
    public Action OnDiceRollEnded;

    protected void Start() {
        DontDestroyOnLoad(gameObject);
        splineContainer = FindFirstObjectByType<SplineContainer>();
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
        var dicePosition = transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.CompareTag("DicePosition"));
        this.dice.transform.SetParent(dicePosition, false);
        visualHandler = GetComponent<BoardPlayerVisualHandler>().Initialize(this.character, this.dice);
    }

    [ClientRpc]
    private void RpcTransferCharacterSelection(LobbyPlayer lobbyPlayer) {
        TransferCharacterSelection(lobbyPlayer);
    }

    [Command]
    public void CmdClaimQuizReward(int amount) {
        PlayerStats.ModifyCoins(amount);
    }

    [Server]
    public WaitUntil TriggerBlockingAnimation(AnimationType animationType, int amount) {
        IsAnimationFinished = false;
        RpcTriggerBlockingAnimation(animationType, amount);
        return new WaitUntil(() => IsAnimationFinished);
    }

    [ClientRpc]
    private void RpcTriggerBlockingAnimation(AnimationType animationType, int amount) {
        StartCoroutine(TriggerAnimationCoroutine());

        IEnumerator TriggerAnimationCoroutine() {
            var waitWhile = visualHandler.TriggerBlockingAnimation(animationType, amount);
            yield return null;
            yield return waitWhile;

            if (isLocalPlayer) { CmdAnimationComplete(); }
        }
    }

    [Command]
    private void CmdAnimationComplete() {
        IsAnimationFinished = true;
    }

    [Command(requiresAuthority = false)]
    private void CmdRollSequenceFinished() {
        rollSequenceFinished = true;
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
            playerMovement.IsMoving = false;
            Transform startPosition;

            if (isFirstLoad) {
                startPosition = GameManager.Singleton.GetStartPosition();
                isFirstLoad = false;
            }
            else {
                startPosition = BoardContext.Instance.FieldBehaviourList.Find(splineKnotIndex).gameObject.transform;
            }

            gameObject.transform.position = startPosition.position;
        }
    }

    [Server]
    protected override void OnServerCleanup() {
        Debug.Log($"[Server] BoardPlayer {name} cleaned up");
        // Stop any ongoing movement
        StopAllCoroutines();
        playerMovement.IsMoving = false;
    }

    [Server]
    protected override void HandleMinigameRewards(IMinigameRewardHandler handler) {
        handler.HandleMinigameRewards(this);
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive) {
            if (!BoardOverlay.Instance.IsPlayerAdded(PlayerId)) {
                BoardOverlay.Instance.AddPlayer(this);
            }
            BoardOverlay.Instance.UpdateRemotePlayerHealth(PlayerStats.GetHealth(), PlayerId);
            BoardOverlay.Instance.UpdateRemotePlayerCoins(PlayerStats.GetCoins(), PlayerId);
        }
        else if (isLocalPlayer && isActive) {
            BoardOverlay.Instance.UpdateLocalPlayerName(PlayerName);
            BoardOverlay.Instance.UpdateLocalPlayerHealth(PlayerStats.GetHealth());
            BoardOverlay.Instance.UpdateLocalPlayerCoins(PlayerStats.GetCoins());
        }
    }

    public override void OnStopClient() {
        base.OnStopClient();
        BoardOverlay.Instance.RemovePlayer(this);
    }

    [Command]
    public void CmdToggleBoardOverview() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || playerMovement.IsMoving || dice.IsSpinning) {
            return;
        }
        CameraHandler.Instance.RpcToggleBoardOverview();
    }

    [Command]
    public void CmdToggleDiceRoll() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || playerMovement.IsMoving) {
            return;
        }
        if (dice.IsSpinning) {
            dice.IsSpinning = false;
            RpcEndDiceCancel();
            return;
        }
        dice.IsSpinning = true;
        RpcStartDiceRoll();
    }

    [Command]
    public void CmdEndRollDice() {
        if (!IsActiveForCurrentScene || !BoardContext.Instance.IsPlayerTurn(this) || playerMovement.IsMoving) {
            return;
        }

        var diceValue = dice.RandomValue;
        RpcStartRollSequence(diceValue);
        rollSequenceFinished = false;
        dice.IsSpinning = false;
        StartCoroutine(WaitForRollSequence());

        IEnumerator WaitForRollSequence() {
            yield return new WaitUntil(() => rollSequenceFinished);
            BoardContext.Instance.ProcessDiceRoll(this, diceValue);
        }
    }

    [ClientRpc]
    private void RpcStartRollSequence(int diceValue) {
        StartCoroutine(visualHandler.StartRollSequence(diceValue, CmdRollSequenceFinished));
    }

    [ClientRpc]
    private void RpcEndDiceCancel() {
        StartCoroutine(visualHandler.OnRollCancel());
    }

    [ClientRpc]
    private void RpcStartDiceRoll() {
        StartCoroutine(visualHandler.OnRollStart());
    }

    [ClientRpc]
    public void RpcUpdateDiceResultLabel(string value) {
        visualHandler.DiceResultLabel(value);
    }

    [ClientRpc]
    public void RpcHideDiceResultLabel() {
        visualHandler.HideDiceResultLabel();
    }

    void Update() {
        if (playerMovement.IsMoving) { return; }
        if (isLocalPlayer) { HandleInput(); }

        visualHandler?.MakeCharacterFaceCamera();
    }

    private void HandleInput() {
        var pressedSpaceToEndRoll = Input.GetKeyDown(KeyCode.Space) && dice.IsSpinning;
        if (pressedSpaceToEndRoll) {
            OnDiceRollEnded?.Invoke();
            CmdEndRollDice();
            return;
        }
    }
}