using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        yield return RpcShowAndHideCatastropheInfo(null);
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
        if (!CameraHandler.Instance.IsZoomedIn) {
            CameraHandler.Instance.RpcZoomIn();
            yield return new WaitForSeconds(CameraHandler.Instance.PlayerToZoomBlendTime + .25f);
        }
        var affectedPlayers = GetAffectedPlayersWithinRange(Vector3.zero, 0);
        var info = IsGlobal() ? FormatDamageInfo() : FormatAffectedPlayers(affectedPlayers);
        yield return RpcShowAndHideCatastropheInfo(info, GetCurrentRoundModalDescriptionId());
        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnCurrentPlayer();

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(CameraHandler.Instance.ZoomToPlayerBlendTime + .25f);

        BoardContext.Instance.UpdateEnvironmentStat(-GetCurrentRoundEnvironmentDamage());
        BoardContext.Instance.UpdateResourceStat(-GetCurrentRoundDamageResources());
        BoardContext.Instance.UpdateEconomyStat(-GetCurrentRoundDamageEconomy());
    }

    private string FormatDamageInfo() {
        var info = "";
        var healthDamage = GetCurrentRoundHealthDamage();
        var environmentDamage = GetCurrentRoundEnvironmentDamage();
        var resourcesDamage = GetCurrentRoundDamageResources();
        var economyDamage = GetCurrentRoundDamageEconomy();

        if (healthDamage > 0) {
            info += $"- {healthDamage} Player Health\n";
        }
        if (environmentDamage > 0) {
            info += $"- {environmentDamage} Environment\n";
        }
        if (resourcesDamage > 0) {
            info += $"- {resourcesDamage} Resources\n";
        }
        if (economyDamage > 0) {
            info += $"- {economyDamage} Economy\n";
        }

        return info.TrimEnd();
    }

    private string FormatAffectedPlayers(List<AffectedPlayerData> affectedPlayers) {
        return affectedPlayers.Select(p => p.ToString()).Aggregate("", (a, b) => a + (string.IsNullOrEmpty(a) ? b : "\n" + b));
    }

    protected IEnumerator RpcShowAndHideCatastropheInfo(string affectedPlayerInfo, long descriptionId = -1) {
        if (descriptionId == -1) {
            descriptionId = GetEndDescriptionId();
        }
        CatastropheManager.Instance.RpcShowCatastropheInfo(affectedPlayerInfo, descriptionId, this);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        CatastropheManager.Instance.RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);
    }

    protected IEnumerator EnsureCameraOnCurrentPlayer() {
        return CatastropheManager.Instance.EnsureCameraOnCurrentPlayer();
    }
}