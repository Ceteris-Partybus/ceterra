using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CatastropheEffect {
    protected int remainingRounds;

    public int RemainingRounds {
        get => remainingRounds;
        set => remainingRounds = value;
    }

    protected CatastropheEffect(int rounds) {
        this.remainingRounds = rounds;
    }

    public bool HasEnded() => remainingRounds == 0;

    public abstract IEnumerator OnCatastropheRages();
    protected abstract IEnumerator OnRaging();
    public abstract IEnumerator OnCatastropheEnds();

    public IEnumerator Tick() {
        yield return OnRaging();
        remainingRounds--;
    }

    protected IEnumerator ApplyDamageToPlayers(List<AffectedPlayerData> affectedPlayers) {
        yield return CatastropheManager.Instance.ApplyDamageToPlayers(affectedPlayers);
    }

    protected List<AffectedPlayerData> GetAffectedPlayersGlobal(int damage) {
        return CatastropheManager.Instance.GetAffectedPlayersGlobal(damage);
    }

    protected List<AffectedPlayerData> GetAffectedPlayersWithinRange(Vector3 center, float effectRadius) {
        return CatastropheManager.Instance.GetAffectedPlayersWithinRange(center, effectRadius, CalculateDamageByDistance);
    }

    protected virtual int CalculateDamageByDistance(float normalizedDistance) {
        return 0;
    }

    protected void RpcShowCatastropheInfo(string affectedPlayerInfo, long descriptionId, CatastropheType catastropheType) {
        CatastropheManager.Instance.RpcShowCatastropheInfo(affectedPlayerInfo, descriptionId, catastropheType);
    }

    protected void RpcHideCatastropheInfo() {
        CatastropheManager.Instance.RpcHideCatastropheInfo();
    }

    protected IEnumerator EnsureCameraOnCurrentPlayer() {
        return CatastropheManager.Instance.EnsureCameraOnCurrentPlayer();
    }
}