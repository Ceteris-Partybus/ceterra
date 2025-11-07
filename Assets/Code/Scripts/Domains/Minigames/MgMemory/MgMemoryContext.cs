using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mirror;
using UnityEngine;

public class MgMemoryContext : NetworkedSingleton<MgMemoryContext> {

    [Header("Minigame Settings")]
    [SerializeField] private float GameDuration;
    [SerializeField] private float scoreboardDuration;
    //[SerializeField] private MgMemoryGameController memoryGameController;

    private float countdownTimer;
    private Coroutine countdownCoroutine;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    private int currentPlayerId = -1;

    private List<MgMemoryPlayer> players;

    protected override void Start() {
        base.Start();
        if (isServer) {
            StartCoroutine(MemoryRoutine());
            countdownCoroutine = StartCoroutine(UpdateCountdown());
        }
    }

    [Server]
    private void InitializePlayers() {
        players = FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(p => p.IsActiveForCurrentScene)
                .ToList();

        RpcInitializePlayersOnClients();
        currentPlayerId = 1;
    }

    [ClientRpc]
    private void RpcInitializePlayersOnClients() {
        players = FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(p => p.IsActiveForCurrentScene)
                .ToList();
    }

    private void OnCurrentPlayerChanged(int _, int newPlayerId) {
        MgMemoryController.Instance.UpdateCurrentPlayer(GetPlayerById(newPlayerId).PlayerName);
    }

    public MgMemoryPlayer GetCurrentPlayer() {
        return GetPlayerById(currentPlayerId);
    }

    public MgMemoryPlayer GetPlayerById(int playerId) {
        return players.Where(p => p.PlayerId == playerId).FirstOrDefault();
    }

    [Server]
    public void HandleMatch() {
        // Bei einem Match bleibt der aktuelle Spieler dran
        Debug.Log($"Player {currentPlayerId} made a match and continues");
    }

    [Server]
    public void HandleMismatch() {
        NextPlayer();
    }

    [Server]
    private void NextPlayer() {
        currentPlayerId = (currentPlayerId % players.Count) + 1;
    }

    private IEnumerator MemoryRoutine() {
        yield return new WaitForSeconds(0.5f);
        InitializePlayers();
        StartMemory();
        yield return new WaitForSeconds(GameDuration);

        RpcClearMemoryOnClients();
        yield return new WaitForSeconds(0.1f);
        MgMemoryController.Instance.ShowScoreboard();
        yield return new WaitForSeconds(scoreboardDuration);

        StopMemory();
        GameManager.Singleton.EndMinigame();
    }

    [Server]
    private void StartMemory() {
        var randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        MgMemoryGameController.Instance.InitializeCardsOnClients(randomSeed);
        RpcInitializeCardsOnClients(randomSeed);

        StartCoroutine(DelayedTurnDisplayInit());
    }

    private IEnumerator DelayedTurnDisplayInit() {
        yield return new WaitForSeconds(0.2f);
        RpcInitializeTurnDisplayOnClients();
    }

    [ClientRpc]
    private void RpcInitializeCardsOnClients(int randomSeed) {
        MgMemoryGameController.Instance.InitializeCardsOnClients(randomSeed);
    }

    [ClientRpc]
    private void RpcInitializeTurnDisplayOnClients() {
        MgMemoryController.Instance.UpdateCurrentPlayer(GetCurrentPlayer().PlayerName);
    }

    [ClientRpc]
    private void RpcClearMemoryOnClients() {
        MgMemoryGameController.Instance.ClearMemory();
    }

    [Server]
    private IEnumerator UpdateCountdown() {
        countdownTimer = GameDuration;
        var lastSeconds = Mathf.CeilToInt(countdownTimer);

        RpcUpdateCountdown(lastSeconds);

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            var seconds = Mathf.CeilToInt(Mathf.Max(0f, countdownTimer));
            if (seconds != lastSeconds) {
                RpcUpdateCountdown(seconds);
                lastSeconds = seconds;
            }
            yield return null;
        }
        RpcUpdateCountdown(0);
        countdownCoroutine = null;
    }

    [ClientRpc]
    private void RpcUpdateCountdown(int seconds) {
        MgMemoryController.Instance.UpdateCountdown(seconds);
    }

    [Server]
    public void StopMemory() {
        if (countdownCoroutine != null) {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }
}