
using Mirror;

public class CatastropheManager : NetworkedSingleton<CatastropheManager> {
    protected override bool ShouldPersistAcrossScenes => true;
    private readonly SyncList<CatastropheEffect> ongoingCatastrophes = new();

    protected override void Start() {
        BoardContext.Instance.OnNewRoundStarted += Tick;
        base.Start();
    }

    [Server]
    public void RegisterCatastrophe(CatastropheType type) {
        var effect = type.CreateEffect();
        ongoingCatastrophes.Add(effect);
        Tick(effect);
    }

    [Server]
    private void Tick() {
        foreach (var catastrophe in ongoingCatastrophes) {
            Tick(catastrophe);
        }
    }

    private void Tick(CatastropheEffect catastrophe) {
        if (catastrophe.HasEnded()) {
            ongoingCatastrophes.Remove(catastrophe);
            catastrophe.OnCatastropheEnds();
            return;
        }
        catastrophe.Tick();
    }
}