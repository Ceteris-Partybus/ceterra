using Mirror;
using System;
using UnityEngine;

public class MgOceanPlayer : SceneConditionalPlayer {
    [SerializeField]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    [SerializeField]
    private GameObject playerModel;

    private void OnScoreChanged(int old, int new_) {
        Debug.Log($"[MgOceanPlayer {PlayerId}] OnScoreChanged from {old} to {new_}. IsLocalPlayer: {isLocalPlayer}");
        if (isLocalPlayer) {
            MgOceanLocalPlayerHUD.Instance?.UpdateScore(new_);
        } else {
            MgOceanRemotePlayerHUD.Instance?.UpdatePlayerScore(PlayerId, new_);
        }
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);
        Debug.Log($"[MgOceanPlayer {PlayerId}] OnClientActiveStateChanged: {isActive}. IsLocalPlayer: {isLocalPlayer}");

        if (!isLocalPlayer && isActive && MgOceanRemotePlayerHUD.Instance != null) {
            if (!MgOceanRemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
                MgOceanRemotePlayerHUD.Instance.AddPlayer(this);
            }
            MgOceanRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, score);
        }

        if (isActive && isLocalPlayer) {
            CmdSpawnPlayer(connectionToClient);
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

    [Server]
    protected override void OnServerInitialize() {
        score = 0;
    }

    public override void OnStopClient() {
        base.OnStopClient();
        MgOceanRemotePlayerHUD.Instance?.RemovePlayer(PlayerId);
    }

    [Command]
    private void CmdSpawnPlayer(NetworkConnectionToClient conn = null) {
        Debug.Log($"Spawning player model for player {PlayerId}");
        Vector3 spawnPosition = MgOceanContext.Instance.GetPlayerSpawnPosition();
        var model = Instantiate(playerModel, spawnPosition, Quaternion.identity);
        
        NetworkServer.Spawn(model, conn);
        Debug.Log($"Player model spawned at {model.transform.position} with tag: {model.tag}");
    }

    [Command]
    public void CmdAddScore(int points) {
        ServerAddScore(points);
    }

    [Server]
    public void ServerAddScore(int points) {
        score += points;
    }

    [Server]
    public void ServerReduceScore(int points) {
        score -= points;
    }   

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgOcean";
    }

    public int Score => score;
}