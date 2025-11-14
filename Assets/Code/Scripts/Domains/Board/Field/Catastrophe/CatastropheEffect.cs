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

    public abstract long GetEndDescriptionId();
    public abstract long GetDisplayNameId();
    public abstract bool IsGlobal();
    protected abstract int GetCurrentRoundHealthDamage();
    protected abstract int GetCurrentRoundEnvironmentDamage();
    protected abstract int GetCurrentRoundDamageResources();
    protected abstract int GetCurrentRoundDamageEconomy();
    protected abstract long GetCurrentRoundModalDescriptionId();

    protected virtual IEnumerator Start() {
        yield break;
    }

    protected virtual IEnumerator Rage() {
        yield break;
    }

    protected virtual IEnumerator End() {
        yield break;
    }

    public IEnumerator OnStart() {
        yield return Start();
        yield return ApplyDamage();
        remainingRounds--;
    }

    public IEnumerator OnRage() {
        yield return Rage();
        yield return ApplyDamage();
        remainingRounds--;
    }

    public IEnumerator OnEnd() {
        RpcShowCatastropheInfo(null);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);
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

    private IEnumerator ApplyDamage() {
        yield return CameraHandler.Instance.ZoomIn();
        var affectedPlayers = IsGlobal() ? GetAffectedPlayersGlobal(GetCurrentRoundHealthDamage()) : GetAffectedPlayersWithinRange(Vector3.zero, 0);
        //affectedPlayers.Select(p => p.ToString()).Aggregate((a, b) => a + "\n" + b), 
        RpcShowCatastropheInfo("", GetCurrentRoundModalDescriptionId());
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);

        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnCurrentPlayer();

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(1f);

        BoardContext.Instance.UpdateEnvironmentStat(-GetCurrentRoundEnvironmentDamage());
        BoardContext.Instance.UpdateResourceStat(-GetCurrentRoundDamageResources());
        BoardContext.Instance.UpdateEconomyStat(-GetCurrentRoundDamageEconomy());
    }

    protected void RpcShowCatastropheInfo(string affectedPlayerInfo, long descriptionId) {
        CatastropheManager.Instance.RpcShowCatastropheInfo(affectedPlayerInfo, descriptionId, this);
    }

    protected void RpcShowCatastropheInfo(string affectedPlayerInfo) {
        CatastropheManager.Instance.RpcShowCatastropheInfo(affectedPlayerInfo, this.GetEndDescriptionId(), this);
    }

    protected void RpcHideCatastropheInfo() {
        CatastropheManager.Instance.RpcHideCatastropheInfo();
    }

    protected IEnumerator EnsureCameraOnCurrentPlayer() {
        return CatastropheManager.Instance.EnsureCameraOnCurrentPlayer();
    }
}