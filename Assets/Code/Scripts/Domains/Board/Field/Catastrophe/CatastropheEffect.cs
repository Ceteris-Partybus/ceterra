public abstract class CatastropheEffect {
    protected int remainingRounds;

    public int RemainingRounds {
        get => remainingRounds;
        set => remainingRounds = value;
    }

    protected CatastropheEffect(int rounds) {
        this.remainingRounds = rounds;
    }

    public void Tick() {
        OnEffectTriggered();
        remainingRounds--;
    }

    public bool HasEnded() => remainingRounds == 0;

    protected abstract void OnEffectTriggered();
}