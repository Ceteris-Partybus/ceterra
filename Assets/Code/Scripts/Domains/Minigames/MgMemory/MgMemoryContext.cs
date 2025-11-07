using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mirror;
using UnityEngine;

public class MgMemoryContext : MgContext<MgMemoryContext, MgMemoryPlayer> {

    [Header("Minigame Settings")]
    [SerializeField] private float GameDuration;
    [SerializeField] private float scoreboardDuration;
    [SerializeField] private string memoryFactsFileName = "Data/Productiondata/Minigames/Memory/memory_facts";
    //[SerializeField] private MgMemoryGameController memoryGameController;

    private float countdownTimer;
    private Coroutine countdownCoroutine;
    private List<MemoryFactData> memoryFacts = new();

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    private int currentPlayerId = -1;

    private List<MgMemoryPlayer> players;

    public override void OnStartGame() {
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
        currentPlayerId = 0;
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
    public void HandleMismatch() {
        NextPlayer();
    }

    [Server]
    private void NextPlayer() {
        currentPlayerId = (currentPlayerId + 1) % players.Count;
    }

    private IEnumerator MemoryRoutine() {
        yield return new WaitForSeconds(0.5f);
        InitializePlayers();
        StartMemory();
        yield return new WaitForSeconds(GameDuration);

        RpcClearMemoryOnClients();
        yield return new WaitForSeconds(0.1f);

        var allActivePlayers = FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(p => p.IsActiveForCurrentScene)
            .OrderByDescending(p => p.Score)
            .ToList();

        var rankings = new List<MgMemoryPlayerRankingData>();
        for (var i = 0; i < allActivePlayers.Count; i++) {
            var player = allActivePlayers[i];
            var rank = i + 1;
            var reward = CalculateCoinReward(rank);

            player.SetEarnedCoinReward(reward);

            rankings.Add(MgMemoryPlayerRankingData.FromPlayer(player, rank));
        }

        MgMemoryController.Instance.ShowScoreboard(rankings);
        yield return new WaitForSeconds(scoreboardDuration);

        StopMemory();
        GameManager.Singleton.EndMinigame();
    }

    [Server]
    private void StartMemory() {
        var randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        var memoryFacts = LoadMemoryCardFacts();

        MgMemoryGameController.Instance.InitializeCardsOnClients(randomSeed, memoryFacts);
        RpcInitializeCardsOnClients(randomSeed, memoryFacts);

        StartCoroutine(DelayedTurnDisplayInit());
    }

    private List<MemoryFactData> LoadMemoryCardFacts() {
        var jsonFile = Resources.Load<TextAsset>(memoryFactsFileName);
        var memoryFactsData = JsonConvert.DeserializeObject<List<MemoryFactData>>(jsonFile.text);
        memoryFacts = memoryFactsData
            .OrderBy(x => UnityEngine.Random.Range(0f, 1f))
            .Take(12)
            .ToList();

        return memoryFacts;
    }

    private IEnumerator DelayedTurnDisplayInit() {
        yield return new WaitForSeconds(0.2f);
        RpcInitializeTurnDisplayOnClients();
    }

    [ClientRpc]
    private void RpcInitializeCardsOnClients(int randomSeed, List<MemoryFactData> memoryFacts) {
        MgMemoryGameController.Instance.InitializeCardsOnClients(randomSeed, memoryFacts);
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

    private int CalculateCoinReward(int rank) {
        return 100 / (int)Mathf.Pow(2, rank - 1);
    }
}