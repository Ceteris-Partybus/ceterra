
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatastropheManager : NetworkedSingleton<CatastropheManager> {
    private const float PLAYER_DAMAGE_DELAY = .3f;
    protected override bool ShouldPersistAcrossScenes => true;
    private readonly SyncList<CatastropheEffect> ongoingCatastrophes = new();

    [Server]
    public IEnumerator RegisterCatastrophe(CatastropheType type) {
        var catastrophe = type.CreateEffect();
        ongoingCatastrophes.Add(catastrophe);
        yield return catastrophe.OnStart();
    }

    [Server]
    public IEnumerator Tick() {
        foreach (var catastrophe in ongoingCatastrophes) {
            if (catastrophe.HasEnded()) {
                ongoingCatastrophes.Remove(catastrophe);
                yield return catastrophe.OnEnd();
                continue;
            }
            yield return catastrophe.OnRage();
        }
    }

    [Server]
    public IEnumerator ApplyDamageToPlayers(List<AffectedPlayerData> affectedPlayers) {
        foreach (var (affectedPlayer, _, inflictedDamage) in affectedPlayers) {
            if (affectedPlayers.Count > 1) {
                CameraHandler.Instance.HasReachedTarget = false;
                CameraHandler.Instance.RpcSwitchZoomTarget(affectedPlayer);
                yield return new WaitUntil(() => CameraHandler.Instance.HasReachedTarget);
            }

            affectedPlayer.IsAnimationFinished = false;
            affectedPlayer.PlayerStats.ModifyHealth(-inflictedDamage);
            yield return affectedPlayer.TriggerBlockingAnimation(AnimationType.HEALTH_LOSS, inflictedDamage);
            yield return new WaitForSeconds(PLAYER_DAMAGE_DELAY);
        }
    }

    [Server]
    public List<AffectedPlayerData> GetAffectedPlayersGlobal(int damage) {
        return BoardContext.Instance.GetAllPlayers()
                    .Select(player => new AffectedPlayerData(player, damage))
                    .ToList();
    }

    [Server]
    public List<AffectedPlayerData> GetAffectedPlayersWithinRange(Vector3 center, float effectRadius, System.Func<float, int> calculateDamageByDistance) {
        return BoardContext.Instance.GetAllPlayers()
                    .Select(player => {
                        var distance = Vector3.Distance(player.transform.position, center);
                        var damage = calculateDamageByDistance(distance / effectRadius);
                        return new AffectedPlayerData(player, distance, damage);
                    })
                    .Where(result => result.Distance <= effectRadius)
                    .OrderByDescending(result => result.Distance)
                    .ToList();
    }

    [ClientRpc]
    public void RpcShowCatastropheInfo(List<AffectedPlayerData> affectedPlayers, long descriptionId, CatastropheEffect catastrophe) {
        StartCoroutine(WaitForInitialization());

        IEnumerator WaitForInitialization() {
            yield return new WaitUntil(() => CatastropheModal.Instance != null);

            CatastropheModal.Instance.Title = LocalizationManager.Instance.GetLocalizedText(catastrophe.GetDisplayNameId());
            CatastropheModal.Instance.Description = LocalizationManager.Instance.GetLocalizedText(descriptionId);
            CatastropheModal.Instance.AffectedPlayers = catastrophe.FormatDamageInfo(affectedPlayers);
            ModalManager.Instance.Show(CatastropheModal.Instance);
        }
    }

    [ClientRpc]
    public void RpcHideCatastropheInfo() {
        ModalManager.Instance.Hide();
    }

    [Server]
    public IEnumerator EnsureCameraOnCurrentPlayer() {
        var currentPlayer = BoardContext.Instance.GetCurrentPlayer();
        if (CameraHandler.Instance.ZoomTarget != currentPlayer.transform) {
            CameraHandler.Instance.RpcSwitchZoomTarget(currentPlayer);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => CameraHandler.Instance.HasReachedTarget);
        }
    }
}