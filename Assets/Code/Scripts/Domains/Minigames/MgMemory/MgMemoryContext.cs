using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mirror;
using UnityEngine;

public class MgMemoryContext : MgContext<MgMemoryContext, MgMemoryPlayer> {
    private float scoreboardDuration;
    private float factPopupDuration = 10f;
    private string memoryFactsFileName = "Data/Productiondata/Minigames/Memory/memory_facts";
    private int totalPairs = 12;

    private List<MemoryFactData> memoryFacts = new();
    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    private int currentPlayerId = -1;

    private List<MgMemoryPlayer> players;

    public override void OnStartGame() {
        if (isServer) {
            StartCoroutine(MemoryRoutine());
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
    public void ShowFactPopupWithDuration(MemoryFactData factData) {
        MgMemoryController.Instance.ShowFactPopup(factData, factPopupDuration);
    }

    [Server]
    private bool IsGameFinished() {
        var totalMatches = players.Sum(p => p.Score);
        return totalMatches >= totalPairs;
    }

    [Server]
    private void NextPlayer() {
        currentPlayerId = (currentPlayerId + 1) % players.Count;
    }

    private IEnumerator MemoryRoutine() {
        yield return new WaitForSeconds(0.5f);
        InitializePlayers();
        StartMemory();

        yield return new WaitUntil(() => IsGameFinished());

        yield return new WaitForSeconds(1f);

        RpcClearMemoryOnClients();
        yield return new WaitForSeconds(0.1f);

        MgRewardService.Instance.DistributeRewards();
        yield return new WaitForSeconds(scoreboardDuration);

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
}