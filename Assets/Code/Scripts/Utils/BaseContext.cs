using Mirror;
using System.Linq;

public abstract class BaseContext<T, P> : NetworkedSingleton<T>
    where T : BaseContext<T, P>
    where P : Player {
    protected int[] playerIds;
    public int[] PlayerIds => playerIds;

    protected override void Start() {
        base.Start();
        playerIds = LobbyManager.singleton.GetPlayerIds().ToArray();
    }

    private NetworkIdentity[] GetSpawnedObjects() {
        var spawnedObjects = NetworkServer.active ? NetworkServer.spawned : NetworkClient.spawned;
        return spawnedObjects.Values.ToArray();
    }

    public P GetPlayerById(int id) {
        foreach (var identity in GetSpawnedObjects()) {
            if (identity.TryGetComponent<P>(out var player)) {
                if (player.Id == id) {
                    return player;
                }
            }
        }
        return null;
    }

    public P[] GetNonLocalPlayers() {
        P[] nonLocalPlayers = new P[playerIds.Count() - 1];
        P localPlayer = GetLocalPlayer();

        int i = 0;

        foreach (var identity in GetSpawnedObjects()) {
            if (identity.TryGetComponent<P>(out var player)) {
                if (player.Id != localPlayer.Id) {
                    nonLocalPlayers[i++] = player;
                }
            }
        }
        return null;
    }

    public P GetLocalPlayer() {
        foreach (var identity in GetSpawnedObjects()) {
            if (identity.TryGetComponent<P>(out var player)) {
                // TODO: This might need to check for authority instead of localPlayer because I think clients just have authority over minigameplayer
                // TODO: But the minigameplayer isn't the localplayer
                // TODO: Ask AI ig
                if (identity.isLocalPlayer) {
                    return player;
                }
            }
        }
        return null;
    }
}