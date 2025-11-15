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

    public abstract CatastropheType GetCatastropheType();
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
        yield return RpcShowAndHideCatastropheInfo(GetEndDescriptionId());
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
        CameraHandler.Instance.RpcZoomIn();
        var affectedPlayers = IsGlobal() ? GetAffectedPlayersGlobal(GetCurrentRoundHealthDamage()) : GetAffectedPlayersWithinRange(Vector3.zero, 0);
        yield return RpcShowAndHideCatastropheInfo(GetCurrentRoundModalDescriptionId(), affectedPlayers);
        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnCurrentPlayer();

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(CameraHandler.Instance.ZoomToPlayerBlendTime + .25f);

        BoardContext.Instance.UpdateEnvironmentStat(-GetCurrentRoundEnvironmentDamage());
        BoardContext.Instance.UpdateResourceStat(-GetCurrentRoundDamageResources());
        BoardContext.Instance.UpdateEconomyStat(-GetCurrentRoundDamageEconomy());
    }

    public string FormatDamageInfo(List<AffectedPlayerData> affectedPlayers) {
        var noDamage = LocalizationManager.Instance.GetLocalizedText(67188403738329088);
        if (HasEnded()) {
            return noDamage;
        }

        var info = "";
        var healthDamage = GetCurrentRoundHealthDamage();
        var environmentDamage = GetCurrentRoundEnvironmentDamage();
        var resourcesDamage = GetCurrentRoundDamageResources();
        var economyDamage = GetCurrentRoundDamageEconomy();

        if (healthDamage > 0) {
            info += IsGlobal() ? $"- {healthDamage} {LocalizationManager.Instance.GetLocalizedText(56153847255523328)}" : FormatAffectedPlayers(affectedPlayers);
        }
        if (environmentDamage > 0) {
            info += $"\n- {environmentDamage} {LocalizationManager.Instance.GetLocalizedText(56146686244806656)}";
        }
        if (resourcesDamage > 0) {
            info += $"\n- {resourcesDamage} {LocalizationManager.Instance.GetLocalizedText(56155672947974144)}";
        }
        if (economyDamage > 0) {
            info += $"\n- {economyDamage} {LocalizationManager.Instance.GetLocalizedText(56146825365676032)}";
        }

        return string.IsNullOrEmpty(info) ? noDamage : info;
    }

    private string FormatAffectedPlayers(List<AffectedPlayerData> affectedPlayers) {
        return affectedPlayers.Select(p => p.ToString()).Aggregate("", (a, b) => a + (string.IsNullOrEmpty(a) ? b : "\n" + b));
    }

    protected IEnumerator RpcShowAndHideCatastropheInfo(long descriptionId, List<AffectedPlayerData> affectedPlayers = null) {
        CatastropheManager.Instance.RpcShowCatastropheInfo(affectedPlayers, descriptionId, this);
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        CatastropheManager.Instance.RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);
    }

    protected IEnumerator EnsureCameraOnCurrentPlayer() {
        return CatastropheManager.Instance.EnsureCameraOnCurrentPlayer();
    }
}