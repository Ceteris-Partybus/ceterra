using Mirror;
using System;
using UnityEngine;

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
        base.OnClientActiveStateChanged(isActive);

        var boardPlayers = FindObjectsByType<BoardPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var bp in boardPlayers) {
            if (isActive) bp.Hide();
            else bp.Show();
        }
    }

    private void OnScoreChanged(int oldScore, int newScore) {
        Debug.Log($"[MgOceanPlayer3D] Score changed: {oldScore} -> {newScore}");
        
        if (isLocalPlayer) {
            // Update local player HUD
            if (MgOcean3DLocalPlayerHUD.Instance != null) {
                MgOcean3DLocalPlayerHUD.Instance.UpdateScore(newScore);
            }
        } else {
            // Update remote player HUD
            if (MgOcean3DRemotePlayerHUD.Instance != null) {
                MgOcean3DRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, newScore);
            }
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

    [Command]
    public void CmdAddScore(int amount) {
        score += amount;
    }

    [Server]
    public void ServerAddScore(int points) {
        score += points;
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