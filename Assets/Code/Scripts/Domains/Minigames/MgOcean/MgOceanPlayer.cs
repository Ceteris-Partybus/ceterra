using Mirror;
using System;
using UnityEngine;

public class MgOceanPlayer : SceneConditionalPlayer {
    [SerializeField]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private uint score;

    [SerializeField]
    private GameObject playerModel;

    private void OnScoreChanged(uint old, uint new_) {
        if (isLocalPlayer) {
            MgOceanLocalPlayerHUD.Instance?.UpdateScore(new_);
        } else {
            MgOceanRemotePlayerHUD.Instance?.UpdatePlayerScore(PlayerId, new_);
        }
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive && MgOceanRemotePlayerHUD.Instance != null) {
            if (!MgOceanRemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
                MgOceanRemotePlayerHUD.Instance.AddPlayer(this);
            }
            MgOceanRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, score);
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

    [Command(requiresAuthority = false)]
    private void CmdSpawnPlayer(NetworkConnectionToClient conn) {
        Debug.Log($"Spawning player model for player {PlayerId}");
        var model = Instantiate(playerModel);
        NetworkServer.Spawn(model, conn);
    }

    public void AddScore(uint points) {
        if (isServer) {
            score += points;
        }
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgOcean";
    }
}