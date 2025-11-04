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

    private List<MgMemoryPlayer> players = new List<MgMemoryPlayer>();

    protected override void Start() {
        base.Start();
        if (isServer) {
            InitializePlayers();
            StartCoroutine(MemoryRoutine());
            countdownCoroutine = StartCoroutine(UpdateCountdown());
        }
    }

    [Server]
    private void InitializePlayers() {
        players.Clear();
        var allPlayers = FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var player in allPlayers) {
            players.Add(player);
        }
        Debug.Log($"Initialized {players.Count} players for Memory minigame.");
        Debug.Log("Players in Memory minigame:");
        foreach (var p in players) {
            Debug.Log($"- Name: {p.PlayerName}");
        }

        if (players.Count > 0) {
            currentPlayerId = 0;
        }
    }

    private void OnCurrentPlayerChanged(int _, int newPlayerId) {
        // Update UI für alle Clients
        var currentPlayer = GetPlayerById(newPlayerId);
        if (currentPlayer != null) {
            var playerName = $"Spieler {newPlayerId + 1}"; // Einfacher Spielername
            MgMemoryController.Instance.UpdateCurrentPlayer(playerName);
        }
    }

    public MgMemoryPlayer GetCurrentPlayer() {
        return GetPlayerById(currentPlayerId);
    }

    public MgMemoryPlayer GetPlayerById(int playerId) {
        // Fallback: wenn PlayerId nicht verfügbar ist, verwende Index
        if (playerId >= 0 && playerId < players.Count) {
            return players[playerId];
        }
        return players.Find(p => p.netId == playerId) ?? (players.Count > 0 ? players[0] : null);
    }

    [Server]
    public void HandleMatch() {
        // Bei einem Match bleibt der aktuelle Spieler dran
        Debug.Log($"Player {currentPlayerId} made a match and continues");
    }

    [Server]
    public void HandleMismatch() {
        // Bei einem Fehler wechselt der Spieler
        NextPlayer();
    }

    [Server]
    private void NextPlayer() {
        if (players.Count == 0) {
            return;
        }

        var nextIndex = (currentPlayerId + 1) % players.Count;
        currentPlayerId = nextIndex;

        Debug.Log($"Turn changed to player {currentPlayerId + 1}");
    }

    private IEnumerator MemoryRoutine() {
        yield return new WaitForSeconds(0.5f);
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
    }

    [ClientRpc]
    private void RpcInitializeCardsOnClients(int randomSeed) {
        MgMemoryGameController.Instance.InitializeCardsOnClients(randomSeed);
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