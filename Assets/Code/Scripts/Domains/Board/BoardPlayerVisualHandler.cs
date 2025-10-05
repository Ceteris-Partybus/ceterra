using UnityEngine;
using System.Collections;
using System;

public class BoardPlayerVisualHandler : MonoBehaviour {
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