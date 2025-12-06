using Mirror;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;

public class MgOceanPlayer3D : SceneConditionalPlayer, IMinigameRewardHandler {
    [Header("Boat Prefab")]
    [SerializeField] private GameObject boatPrefab;

    [Header("Score")]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    private int earnedCoinReward = 0;
    private GameObject spawnedBoat;

    public int playerScore => score;

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgOcean3D";
    }

    [Server]
    protected override void OnServerInitialize() {
        score = 0;
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        StartCoroutine(WaitForAllPlayers());

        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count());
        }

        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive && MgOcean3DRemotePlayerHUD.Instance != null) {
            if (!MgOcean3DRemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
                MgOcean3DRemotePlayerHUD.Instance.AddPlayer(this);
            }
            MgOcean3DRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, score);
        }

        var boardPlayers = FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (isActive) {
            foreach (var bp in boardPlayers) {
                bp.Hide();
            }
        }
        else {
            foreach (var bp in boardPlayers) {
                bp.Show();
            }
        }
    }

    private void OnScoreChanged(int oldScore, int newScore) {
        Debug.Log($"Score changed from {oldScore} to {newScore}");
        if (isLocalPlayer) {
            MgOcean3DLocalPlayerHUD.Instance.UpdateScore(newScore);
        }
        else {
            MgOcean3DRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, newScore);
        }
    }

    [Command]
    public void CmdSpawnBoat() {
        if (boatPrefab == null) {
            Debug.LogError("[MgOceanPlayer3D] Boat prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = MgOceanContext3D.Instance.GetPlayerSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;

        var boat = Instantiate(boatPrefab, spawnPosition, spawnRotation);
        NetworkServer.Spawn(boat, connectionToClient);

        spawnedBoat = boat;
    }

    [Server]
    public void ServerAddScore(int points) {
        score += points;
    }

    [Server]
    public void ServerSubtractScore(int amount) {
        score = Mathf.Max(score - amount, 0);
    }

    [Server]
    public void SetMinigameReward(int reward) {
        earnedCoinReward = reward;
    }

    public void HandleMinigameRewards(BoardPlayer player) {
        player.PlayerStats.ModifyCoins(earnedCoinReward);
        player.PlayerStats.ModifyScore(Math.Max(0, score / 5));
    }
}