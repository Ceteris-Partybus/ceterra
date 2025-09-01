using Mirror;
using System;
using UnityEngine;

public class MgGarbagePlayer : SceneConditionalPlayer {
    [SerializeField]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private uint score;

    private void OnScoreChanged(uint old, uint new_) {
        Debug.Log($"Score changed from {old} to {new_}");
        if (isLocalPlayer) {
            LocalPlayerHUD.Instance.UpdateScore(new_);
        }
        else {
            // if (!RemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
            //     RemotePlayerHUD.Instance.AddPlayer(this);
            // }
            RemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, new_);
        }
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive && RemotePlayerHUD.Instance != null) {
            if (!RemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
                RemotePlayerHUD.Instance.AddPlayer(this);
            }
            RemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, score);
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
        RemotePlayerHUD.Instance.RemovePlayer(PlayerId);
    }

    public uint Score => score;

    [Command]
    public void CmdAddScore(uint amount) {
        score += amount;
    }

    [Command]
    public void CmdSubtractScore(uint amount) {
        score = (uint)Mathf.Max(score - amount, 0);
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgGarbage";
    }
}