using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MgGarbagePlayer : SceneConditionalPlayer, IMinigameRewardHandler {
    [SerializeField]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    private void OnScoreChanged(int old, int new_) {
        Debug.Log($"Score changed from {old} to {new_}");
        if (isLocalPlayer) {
            MgGarbageLocalPlayerHUD.Instance.UpdateScore(new_);
        }
        else {
            // if (!RemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
            //     RemotePlayerHUD.Instance.AddPlayer(this);
            // }
            MgGarbageRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, new_);
        }
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        StartCoroutine(WaitForAllPlayers());

        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count());
        }

        base.OnClientActiveStateChanged(isActive);

        if (!isLocalPlayer && isActive && MgGarbageRemotePlayerHUD.Instance != null) {
            if (!MgGarbageRemotePlayerHUD.Instance.IsPlayerAdded(PlayerId)) {
                MgGarbageRemotePlayerHUD.Instance.AddPlayer(this);
            }
            MgGarbageRemotePlayerHUD.Instance.UpdatePlayerScore(PlayerId, score);
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

    [Command]
    public void CmdAddScore(int amount) {
        score += amount;
    }

    [Command]
    public void CmdSubtractScore(int amount) {
        score = Mathf.Max(score - amount, 0);
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgGarbage";
    }

    [Server]
    public void HandleMinigameRewards(BoardPlayer player) {
        player.PlayerStats.ModifyCoins(Math.Max(0, score));
        player.PlayerStats.ModifyScore(Math.Max(0, score / 5));
    }
}