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

    protected override void Start() {
        base.Start();
        if (isServer) {
            StartCoroutine(MemoryRoutine());
            countdownCoroutine = StartCoroutine(UpdateCountdown());
        }
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