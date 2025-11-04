using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CatastropheEffect {
    private const float PLAYER_DAMAGE_DELAY = .3f;
    protected int remainingRounds;

    public int RemainingRounds {
        get => remainingRounds;
        set => remainingRounds = value;
    }

    protected CatastropheEffect(int rounds) {
        this.remainingRounds = rounds;
    }

    public bool HasEnded() => remainingRounds == 0;

    public abstract void OnCatastropheRages();
    protected abstract void OnRaging();
    public abstract void OnCatastropheEnds();

    public void Tick() {
        OnRaging();
        remainingRounds--;
    }

    [Server]
    private IEnumerator ApplyDamageToPlayers(List<AffectedPlayerData> affectedPlayers) {
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
    private List<AffectedPlayerData> GetAffectedPlayersWithinRange(Vector3 center, float effectRadius) {
        return BoardContext.Instance.GetAllPlayers()
                    .Select(player => {
                        var distance = Vector3.Distance(player.transform.position, center);
                        var damage = CalculateDamageByDistance(distance / effectRadius);
                        return new AffectedPlayerData(player, distance, damage);
                    })
                    .Where(result => result.Distance <= effectRadius)
                    .OrderByDescending(result => result.Distance)
                    .ToList();
    }

    protected virtual int CalculateDamageByDistance(float normalizedDistance) {
        return 0;
    }

    [ClientRpc]
    protected void RpcShowCatastropheInfo(string affectedPlayerInfo, CatastropheType catastropheType) {
        CatastropheModal.Instance.Title = LocalizationManager.Instance.GetLocalizedText(catastropheType.GetDisplayName());
        CatastropheModal.Instance.Description = LocalizationManager.Instance.GetLocalizedText(catastropheType.GetDescription());
        CatastropheModal.Instance.AffectedPlayers = affectedPlayerInfo;
        ModalManager.Instance.Show(CatastropheModal.Instance);
    }

    [ClientRpc]
    protected void RpcHideCatastropheInfo() {
        ModalManager.Instance.Hide();
    }

    [Server]
    private IEnumerator EnsureCameraOnTriggeringPlayer(BoardPlayer triggeringPlayer) {
        if (CameraHandler.Instance.ZoomTarget != triggeringPlayer.transform) {
            CameraHandler.Instance.RpcSwitchZoomTarget(triggeringPlayer);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => CameraHandler.Instance.HasReachedTarget);
        }
    }

    internal class AffectedPlayerData {
        private BoardPlayer player;
        public BoardPlayer Player => player;
        private float distance;
        public float Distance => distance;
        private int inflictedDamage;
        public int InflictedDamage => inflictedDamage;

        internal AffectedPlayerData(BoardPlayer player, float distance, int inflictedDamage) {
            this.player = player;
            this.distance = distance;
            this.inflictedDamage = inflictedDamage;
        }

        internal void Deconstruct(out BoardPlayer player, out float distance, out int inflictedDamage) {
            player = Player;
            distance = Distance;
            inflictedDamage = InflictedDamage;
        }

        public override string ToString() {
            return LocalizationManager.Instance.GetLocalizedText(56668768562413568, new object[] { Player.PlayerName, Distance.ToString("F2"), InflictedDamage });
        }
    }
}