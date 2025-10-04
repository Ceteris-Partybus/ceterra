using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using DG.Tweening;
using System;

public class BoardPlayerVisualHandler : MonoBehaviour {
    [Header("Branch Arrows")]
    [SerializeField] private Transform branchArrowPrefab;
    [SerializeField] private float branchArrowRadius;
    private List<GameObject> branchArrows = new List<GameObject>();

    [Header("Animation Trigger")]
    [SerializeField] private string coinGainTrigger;
    [SerializeField] private string coinLossTrigger;
    [SerializeField] private string healthGainTrigger;
    [SerializeField] private string healthLossTrigger;
    [SerializeField] private string runTrigger;
    [SerializeField] private string diceHitTrigger;
    [SerializeField] private string idleTrigger;
    [SerializeField] private string diceSpinTrigger;
    [SerializeField] private string junctionEntryTrigger;

    private Character character;
    private Dice dice;
    public string DiceResultLabel(string value) => dice.ResultLabel(value);

    public BoardPlayerVisualHandler Initialize(Character character, Dice dice) {
        this.character = character;
        this.dice = dice;
        dice.SetInPreview = false;
        dice.Hide();
        return this;
    }

    public WaitWhile TriggerBlockingAnimation(AnimationType animationType) {
        var particleEffectAndTrigger = animationType switch {
            AnimationType.COIN_GAIN => (character.CoinGainParticle, coinGainTrigger),
            AnimationType.COIN_LOSS => (character.CoinLossParticle, coinLossTrigger),
            AnimationType.HEALTH_GAIN => (character.HealthGainParticle, healthGainTrigger),
            AnimationType.HEALTH_LOSS => (character.HealthLossParticle, healthLossTrigger),
            _ => throw new ArgumentException("Invalid blocking animation type")
        };
        particleEffectAndTrigger.Item1.Play();
        character.Animator.SetTrigger(particleEffectAndTrigger.Item2);
        return new WaitWhile(() => particleEffectAndTrigger.Item1.isPlaying || IsAnimationPlaying(particleEffectAndTrigger.Item2));
    }

    public void TriggerAnimation(AnimationType animationType) {
        character.Animator.SetTrigger(animationType switch {
            AnimationType.DICE_HIT => diceHitTrigger,
            AnimationType.IDLE => idleTrigger,
            AnimationType.DICE_SPIN => diceSpinTrigger,
            AnimationType.JUNCTION_ENTRY => junctionEntryTrigger,
            AnimationType.RUN => runTrigger,
            _ => throw new ArgumentException("Invalid animation type")
        });
    }

    public void HideDiceResultLabel() {
        dice.HideDiceResultLabel();
    }

    public void ShowBranchArrows(IReadOnlyList<FieldBehaviour> nextFields, BoardPlayer player) {
        for (var i = 0; i < nextFields.Count; i++) {
            var branchArrow = InstantiateBranchArrow(nextFields[i], player);
            branchArrow.GetComponent<BranchArrowMouseEventHandler>()?.Initialize(player, i);
            branchArrows.Add(branchArrow);
        }
    }

    public void HideBranchArrows() {
        foreach (var arrow in branchArrows) {
            Destroy(arrow);
        }
        branchArrows.Clear();
    }

    private GameObject InstantiateBranchArrow(FieldBehaviour targetField, BoardPlayer player) {
        var targetSpline = player.SplineContainer.Splines[targetField.SplineKnotIndex.Spline];
        var normalizedPlayerPosition = player.NormalizedSplinePosition;

        if (player.SplineKnotIndex.Spline != targetField.SplineKnotIndex.Spline && targetField.SplineKnotIndex.Knot == 1) {
            normalizedPlayerPosition = 0f;
        }

        var offset = 0.01f;
        var tangent = targetSpline.EvaluateTangent(normalizedPlayerPosition + offset);
        var worldTangent = player.SplineContainer.transform.TransformDirection(tangent).normalized;

        var branchArrowPosition = transform.position + worldTangent * branchArrowRadius;
        return Instantiate(branchArrowPrefab.gameObject, branchArrowPosition, Quaternion.LookRotation(worldTangent, Vector3.up));
    }

    public void OnRollStart() {
        CameraHandler.Instance.ZoomIn();
        dice.OnRollStart();
        TriggerAnimation(AnimationType.DICE_SPIN);
    }

    public void OnRollCancel() {
        dice.OnRollCancel();
        TriggerAnimation(AnimationType.IDLE);
    }

    public IEnumerator StartRollSequence(int diceValue) {
        character.HitDice();
        TriggerAnimation(AnimationType.DICE_HIT);
        yield return new WaitForSeconds(0.09f);

        dice.OnRollDisplay(diceValue);
        yield return new WaitForSeconds(0.5f);

        dice.OnRollEnd(diceValue);
        yield return new WaitForSeconds(0.6f);

        CameraHandler.Instance.ZoomOut();
        yield return new WaitForSeconds(0.5f);
    }

    public void CleanRotation() {
        dice.Particles.transform.rotation = Quaternion.identity;
        character.Particles.transform.rotation = Quaternion.identity;
    }

    private bool IsAnimationPlaying(string trigger) {
        return character.Animator.IsInTransition(0) || character.Animator.GetCurrentAnimatorStateInfo(0).IsName(trigger);
    }

    public void MakeCharacterFaceCamera() {
        character.FaceCamera();
    }

    public void SetMovementRotation(Quaternion targetRotation, float lerpSpeed) {
        character.SetMovementRotation(targetRotation, lerpSpeed);
    }
}