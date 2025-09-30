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
    }

    protected override void OnClientActiveStateChanged(bool isActive) {
        base.OnClientActiveStateChanged(isActive);

        if (isClient && isActive && isLocalPlayer) {
            CmdSpawnPlayer(connectionToClient);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSpawnPlayer(NetworkConnectionToClient conn) {
        Debug.Log($"Spawning player model for player {PlayerId}");
        var model = Instantiate(playerModel);
        NetworkServer.Spawn(model, conn);
    }

    public override bool ShouldBeActiveInScene(string sceneName) {
        return sceneName == "MgOcean";
    }
}