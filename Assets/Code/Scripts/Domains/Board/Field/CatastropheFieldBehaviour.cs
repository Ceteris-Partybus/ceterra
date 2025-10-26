using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatastropheFieldBehaviour : FieldBehaviour {
    private const float PLAYER_DAMAGE_DELAY = 0.3f;

    private static int catastropheFieldCount = 0;
    private static readonly CatastropheType[] catastropheTypes = (CatastropheType[])System.Enum.GetValues(typeof(CatastropheType));

    [SyncVar] private bool hasBeenInvoked = false;
    [SyncVar] private CatastropheType catastropheType;

    private int environmentEffect;
    private int healthEffect;
    private int effectRadius;

    public void Start() {
        catastropheType = catastropheTypes[catastropheFieldCount++ % catastropheTypes.Length];
        var effects = CatastropheTypeExtensions.GetEffects(catastropheType);
        environmentEffect = effects.Item1;
        healthEffect = effects.Item2;
        effectRadius = effects.Item3;
    }

    [Server]
    protected override void OnPlayerLand(BoardPlayer player) {
        if (!hasBeenInvoked) {
            hasBeenInvoked = true;
            StartCoroutine(ProcessCatastropheSequence(player));
        }
        player.PlayerStats.ModifyScore(-1 * healthEffect / 5);
    }

    [Server]
    private IEnumerator ProcessCatastropheSequence(BoardPlayer triggeringPlayer) {
        var affectedPlayers = GetAffectedPlayers(triggeringPlayer);
        RpcShowCatastropheInfo(affectedPlayers.Select(p => p.ToString()).Aggregate((a, b) => a + "\n" + b));
        yield return new WaitForSeconds(Modal.DEFAULT_DISPLAY_DURATION);

        RpcHideCatastropheInfo();
        yield return new WaitForSeconds(.5f);

        yield return ApplyDamageToPlayers(affectedPlayers);
        yield return EnsureCameraOnTriggeringPlayer(triggeringPlayer);

        CameraHandler.Instance.RpcZoomOut();
        yield return new WaitForSeconds(1f);

        var localScale = transform.localScale;
        var scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(Vector3.zero, .5f))
                     .AppendCallback(() => RpcChangeMaterial())
                     .Append(transform.DOScale(localScale, .5f));
        yield return scaleSequence.WaitForCompletion();
        yield return new WaitForSeconds(1f);

        BoardContext.Instance.UpdateEnvironmentStat(environmentEffect);
        CompleteFieldInvocation();

        FieldInstantiate.Instance.ReplaceField(this, FieldType.NORMAL);
    }

    [ClientRpc]
    private void RpcShowCatastropheInfo(string affectedPlayerInfo) {
        CatastropheModal.Instance.Title = LocalizationManager.Instance.GetLocalizedText(catastropheType.GetDisplayName());
        CatastropheModal.Instance.Description = LocalizationManager.Instance.GetLocalizedText(catastropheType.GetDescription());
        CatastropheModal.Instance.AffectedPlayers = affectedPlayerInfo;
        ModalManager.Instance.Show(CatastropheModal.Instance);
    }

    [ClientRpc]
    private void RpcHideCatastropheInfo() {
        ModalManager.Instance.Hide();
    }

    [ClientRpc]
    private void RpcChangeMaterial() {
        GetComponent<Renderer>().sharedMaterials = FieldInstantiate.Instance.NormalFieldMaterial;
    }

    [Server]
    private List<AffectedPlayerData> GetAffectedPlayers(BoardPlayer triggeringPlayer) {
        var center = triggeringPlayer.transform.position;
        return FindObjectsByType<BoardPlayer>(FindObjectsSortMode.None)
                    .Select(player => {
                        var distance = Vector3.Distance(player.transform.position, center);
                        var damage = CalculateDamageByDistance(distance);
                        return new AffectedPlayerData(player, distance, damage);
                    })
                    .Where(result => result.Distance <= effectRadius)
                    .OrderByDescending(result => result.Distance)
                    .ToList();
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

    private int CalculateDamageByDistance(float distance) {
        var normalizedDistance = distance / effectRadius;
        return normalizedDistance switch {
            <= .25f => healthEffect,
            <= .5f => Mathf.RoundToInt(healthEffect * .75f),
            <= .75f => Mathf.RoundToInt(healthEffect * .5f),
            _ => Mathf.RoundToInt(healthEffect * .25f),
        };
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