using Mirror;

public abstract class PlayerDataContext<T, P, D> : BaseContext<T, P>
    where T : PlayerDataContext<T, P, D>
    where P : Player
    where D : new() {
    protected override void Start() {
        base.Start();

        foreach (int id in playerIds) {
            playerData.Add(id, new D());
        }
    }

    protected readonly SyncDictionary<int, D> playerData = new();

    public SyncDictionary<int, D> PlayerData => playerData;
}