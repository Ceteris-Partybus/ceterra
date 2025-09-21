using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Splines;
using DG.Tweening;
using Unity.Cinemachine;
using System;
using Random = UnityEngine.Random;

public class BoardPlayerVisualHandler : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private Transform particles;
    [SerializeField] private ParticleSystem coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;
    [SerializeField] private ParticleSystem healthGainParticle;
    [SerializeField] private ParticleSystem healthLossParticle;
    [SerializeField] private ParticleSystem diceHitParticle;
    [SerializeField] private ParticleSystem diceResultParticle;

    [Header("Dice Parameters")]
    [SerializeField] private Transform playerDice;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float tiltAmplitude;
    [SerializeField] private float tiltFrequency;
    [SerializeField] private float numberAnimationSpeed;
    [SerializeField] private TextMeshPro[] numberLabels;
    [SerializeField] private TextMeshPro diceResultLabel;
    [SerializeField] private AnimationCurve scaleEase;

    [Header("Branch Arrows")]
    [SerializeField] private Transform branchArrowPrefab;
    [SerializeField] private float branchArrowRadius;
    private List<GameObject> branchArrows = new List<GameObject>();

    [Header("Player Parameters")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private int jumpPower = 1;
    [SerializeField] private float jumpDuration = .2f;

    [Header("Animation Parameters")]
    [SerializeField] private Animator animator;
    [SerializeField] private string coinGainTrigger;
    [SerializeField] private string coinLossTrigger;
    [SerializeField] private string healthGainTrigger;
    [SerializeField] private string healthLossTrigger;
    [SerializeField] private string runTrigger;
    [SerializeField] private string jumpTrigger;
    [SerializeField] private string idleTrigger;
    [SerializeField] private string danceTrigger;
    [SerializeField] private string thinkTrigger;

    private bool diceSpinning;
    private float tiltTime = 0f;

    public bool IsDiceSpinning => diceSpinning;
    public string DiceResultLabel {
        set => diceResultLabel.text = value;
    }

    protected void Start() {
        HideDice();
        HideDiceResultLabel();
    }

    public WaitWhile OnCoinGain() {
        coinGainParticle.Play();
        animator.SetTrigger(coinGainTrigger);
        return new WaitWhile(() => coinGainParticle.isPlaying || IsAnimationPlaying(coinGainTrigger));
    }

    public WaitWhile OnCoinLoss() {
        coinLossParticle.Play();
        animator.SetTrigger(coinLossTrigger);
        return new WaitWhile(() => coinLossParticle.isPlaying || IsAnimationPlaying(coinLossTrigger));
    }

    public WaitWhile OnHealthGain() {
        healthGainParticle.Play();
        animator.SetTrigger(healthGainTrigger);
        return new WaitWhile(() => healthGainParticle.isPlaying || IsAnimationPlaying(healthGainTrigger));
    }

    public WaitWhile OnHealthLoss() {
        healthLossParticle.Play();
        animator.SetTrigger(healthLossTrigger);
        return new WaitWhile(() => healthLossParticle.isPlaying || IsAnimationPlaying(healthLossTrigger));
    }

    public void OnRun() {
        animator.SetTrigger(runTrigger);
    }

    public void OnJump() {
        animator.SetTrigger(jumpTrigger);
    }

    public void OnIdle() {
        animator.SetTrigger(idleTrigger);
    }

    public void OnDance() {
        animator.SetTrigger(danceTrigger);
    }

    public void OnThink() {
        animator.SetTrigger(thinkTrigger);
    }

    public WaitWhile TriggerBlockingAnimation(AnimationType animationType) {
        return animationType switch {
            AnimationType.COIN_GAIN => OnCoinGain(),
            AnimationType.COIN_LOSS => OnCoinLoss(),
            AnimationType.HEALTH_GAIN => OnHealthGain(),
            AnimationType.HEALTH_LOSS => OnHealthLoss(),
            _ => new WaitWhile(() => false)
        };
    }

    public void TriggerAnimation(AnimationType animationType) {
        (animationType switch {
            AnimationType.JUMP => OnJump,
            AnimationType.IDLE => OnIdle,
            AnimationType.DANCE => OnDance,
            AnimationType.THINK => OnThink,
            AnimationType.RUN => OnRun,
            _ => new Action(() => { })
        }).Invoke();
    }

    private void ShowDice() {
        playerDice.gameObject.SetActive(true);
    }

    private void HideDice() {
        playerDice.transform.localScale = Vector3.one;
        playerDice.gameObject.SetActive(false);
    }

    public void ShowDiceResultLabel() {
        diceResultLabel.gameObject.SetActive(true);
    }

    public void HideDiceResultLabel() {
        diceResultLabel.gameObject.SetActive(false);
    }

    public void SetDiceNumber(int value) {
        foreach (var label in numberLabels) {
            label.text = value.ToString();
        }
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

    private void Update() {
        if (diceSpinning) { SpinDice(); }

        if (diceResultLabel.gameObject.activeSelf) {
            diceResultLabel.transform.rotation = Quaternion.LookRotation(diceResultLabel.transform.position - Camera.main.transform.position);
        }
    }

    private void SpinDice() {
        playerDice.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        tiltTime += Time.deltaTime * tiltFrequency;
        var tiltAngle = Mathf.Sin(tiltTime) * tiltAmplitude;

        playerDice.rotation = Quaternion.Euler(tiltAngle, playerDice.rotation.eulerAngles.y, 0);
    }

    private IEnumerator RandomDiceNumberCoroutine() {
        if (!diceSpinning) { yield break; }

        var num = Random.Range(1, 11);
        SetDiceNumber(num);
        yield return new WaitForSeconds(numberAnimationSpeed);
        StartCoroutine(RandomDiceNumberCoroutine());
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
        transform.DOLookAt(Camera.main.transform.position, .35f, AxisConstraint.Y);

        diceSpinning = true;

        StartCoroutine(RandomDiceNumberCoroutine());

        ShowDice();
        playerDice.DOScale(0, .3f).From();
        OnDance();
    }

    public void OnRollCancel() {
        diceSpinning = false;
        playerDice.DOComplete();
        playerDice.DOScale(0, .12f).OnComplete(() => HideDice());
        OnIdle();
    }

    public void OnRollJump() {
        playerModel.DOComplete();
        playerModel.DOJump(transform.position, jumpPower, 1, jumpDuration);
        OnJump();
    }

    public void OnRollDisplay(int roll) {
        diceHitParticle.Play();
        if (diceHitParticle.GetComponent<CinemachineImpulseSource>() != null) {
            diceHitParticle.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        }
        playerDice.DOComplete();
        diceSpinning = false;
        SetDiceNumber(roll);
        playerDice.transform.eulerAngles = Vector3.zero;
        var diceLocalPos = playerDice.localPosition;
        playerDice.DOLocalJump(diceLocalPos, .8f, 1, .25f);
        playerDice.DOPunchScale(Vector3.one / 4, .3f, 10, 1);
    }

    public void OnRollEnd(int roll) {
        HideDice();
        diceResultParticle.Play();

        ShowDiceResultLabel();
        diceResultLabel.text = roll.ToString();
        diceResultLabel.transform.DOComplete();
        diceResultLabel.transform.DOScale(0, .2f).From().SetEase(scaleEase);
    }

    public void CleanRotation() {
        particles.transform.rotation = Quaternion.identity;
    }

    private bool IsAnimationPlaying(string trigger) {
        return animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).IsName(trigger);
    }
}