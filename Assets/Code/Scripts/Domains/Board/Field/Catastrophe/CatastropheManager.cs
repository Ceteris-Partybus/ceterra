
using Mirror;

public class CatastropheManager : NetworkedSingleton<CatastropheManager> {
    protected override bool ShouldPersistAcrossScenes => true;
    private SyncList<CatastropheEffect> onGoingCatastrophes = new();

    protected override void Start() {
        BoardContext.Instance.OnNewRoundStarted += Tick;
        base.Start();
    }

    [Server]
    public void RegisterCatastrophe(CatastropheType type) {
        var effect = type.CreateEffect();
        onGoingCatastrophes.Add(effect);
        Tick(effect);
    }

    [Server]
    private void Tick() {
        foreach (var catastrophe in onGoingCatastrophes) {
            Tick(catastrophe);
        }
    }

    private void Tick(CatastropheEffect catastrophe) {
        catastrophe.Tick();
        if (catastrophe.HasEnded()) {
            onGoingCatastrophes.Remove(catastrophe);
        }
    }
}