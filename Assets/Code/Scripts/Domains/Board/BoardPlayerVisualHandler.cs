using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
using TMPro;

public class BoardPlayerVisualHandler : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private Transform characterParticles;
    [SerializeField] private ParticleSystem coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;
    [SerializeField] private ParticleSystem healthGainParticle;
    [SerializeField] private ParticleSystem healthLossParticle;
    [SerializeField] private TextMeshPro resultLabel;

    [Header("Animation Trigger")]
    [SerializeField] private string coinGainTrigger;
    [SerializeField] private string coinLossTrigger;
    [SerializeField] private string healthGainTrigger;
    [SerializeField] private string healthLossTrigger;
    [SerializeField] private string runTrigger;
    [SerializeField] private string jumpTrigger;
    [SerializeField] private string diceHitTrigger;
    [SerializeField] private string idleTrigger;

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

    public WaitWhile TriggerBlockingAnimation(AnimationType animationType, int amount) {
        (ParticleSystem, string, Action) particleEffectAndTrigger = animationType switch {
            AnimationType.COIN_GAIN => (coinGainParticle, coinGainTrigger, () => ShowCoinChange(amount)),
            AnimationType.COIN_LOSS => (coinLossParticle, coinLossTrigger, () => ShowCoinChange(-amount)),
            AnimationType.HEALTH_GAIN => (healthGainParticle, healthGainTrigger, () => ShowHealthChange(amount)),
            AnimationType.HEALTH_LOSS => (healthLossParticle, healthLossTrigger, () => ShowHealthChange(-amount)),
            _ => throw new ArgumentException("Invalid blocking animation type")
        };

        var emission = particleEffectAndTrigger.Item1.emission;
        var burst = emission.GetBurst(0);
        var isGainEffect = animationType == AnimationType.COIN_GAIN || animationType == AnimationType.HEALTH_GAIN;

        if (isGainEffect) {
            burst.cycleCount = amount;
        }
        else {
            burst.count = amount;
        }

        emission.SetBurst(0, burst);

        particleEffectAndTrigger.Item1.Play();
        character.Animator.SetTrigger(particleEffectAndTrigger.Item2);
        particleEffectAndTrigger.Item3();
        return new WaitWhile(() => particleEffectAndTrigger.Item1.isPlaying || IsAnimationPlaying(particleEffectAndTrigger.Item2));
    }

    public void TriggerAnimation(AnimationType animationType) {
        character.Animator.SetTrigger(animationType switch {
            AnimationType.DICE_HIT => diceHitTrigger,
            AnimationType.IDLE => idleTrigger,
            AnimationType.RUN => runTrigger,
            AnimationType.JUMP => jumpTrigger,
            _ => throw new ArgumentException("Invalid animation type")
        });
    }

    public void HideDiceResultLabel() {
        dice.HideDiceResultLabel();
    }

    public IEnumerator OnRollStart() {
        yield return CameraHandler.Instance.ZoomIn();

        dice.OnRollStart();
    }

    public IEnumerator OnRollCancel() {
        yield return CameraHandler.Instance.ZoomOut();

        dice.OnRollCancel();
        TriggerAnimation(AnimationType.IDLE);
    }

    public IEnumerator StartRollSequence(int diceValue, Action cmdRollSequenceFinished) {
        character.HitDice();
        TriggerAnimation(AnimationType.DICE_HIT);
        yield return new WaitForSeconds(.09f);

        dice.OnRollDisplay(diceValue);
        yield return new WaitForSeconds(.5f);

        dice.OnRollEnd(diceValue);
        yield return new WaitForSeconds(1f);

        yield return CameraHandler.Instance.ZoomOut();
        cmdRollSequenceFinished();
    }

    public void CleanRotation() {
        dice.Particles.transform.rotation = Quaternion.identity;
        characterParticles.transform.rotation = Quaternion.identity;
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

    public void ShowCoinChange(int amount) {
        ShowFloatingLabel(amount, "Coins");
    }

    public void ShowHealthChange(int amount) {
        ShowFloatingLabel(amount, "Health");
    }

    private void ShowFloatingLabel(int amount, string type) {
        var sign = amount > 0 ? "+" : "-";
        resultLabel.text = $"{sign}{Mathf.Abs(amount)} {type}";
        ColorUtility.TryParseHtmlString("#30C650", out var greenColor);
        ColorUtility.TryParseHtmlString("#C64030", out var redColor);
        resultLabel.color = amount > 0 ? greenColor : redColor;

        var startPos = resultLabel.transform.position;
        resultLabel.transform
            .DOMoveY(startPos.y + 1f, 1.15f)
            .OnComplete(() => {
                resultLabel.text = "";
                resultLabel.transform.position = startPos;
            });
    }
}