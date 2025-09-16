using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Splines;

public class BoardPlayerVisualHandler : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private ParticleSystem coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;

    [Header("Dice Parameters")]
    [SerializeField] private Transform playerDice;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float tiltAmplitude;
    [SerializeField] private float tiltFrequency;
    [SerializeField] private float numberAnimationSpeed;
    [SerializeField] private TextMeshPro[] numberLabels;
    [SerializeField] private TextMeshPro diceResultLabel;

    [Header("Branch Arrows")]
    [SerializeField] private Transform branchArrowPrefab;
    [SerializeField] private float branchArrowRadius;
    private List<GameObject> branchArrows = new List<GameObject>();

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

    public IEnumerator PlayCoinGainParticle() {
        coinGainParticle.Play();
        yield return new WaitWhile(() => coinGainParticle.isPlaying);
    }

    public IEnumerator PlayCoinLossParticle() {
        coinLossParticle.Play();
        yield return new WaitWhile(() => coinLossParticle.isPlaying);
    }

    private void ShowDice() {
        playerDice.gameObject.SetActive(true);
    }

    private void HideDice() {
        playerDice.gameObject.SetActive(false);
        playerDice.transform.eulerAngles = Vector3.zero;
    }

    public void StartDiceSpinning() {
        diceSpinning = true;
        ShowDice();
        StartCoroutine(RandomDiceNumberCoroutine());
    }

    public void StopDiceSpinning() {
        diceSpinning = false;
        HideDice();
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

    public void CleanRotation() {
        transform.rotation = Quaternion.identity;
    }
}