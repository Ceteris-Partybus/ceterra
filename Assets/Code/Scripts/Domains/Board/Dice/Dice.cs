using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UIElements;

public class Dice : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private Transform particles;
    public Transform Particles => particles;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private ParticleSystem resultParticle;

    [Header("General")]
    [SerializeField] private Transform model;
    [SerializeField] private Texture2D iconTexture2D;
    private StyleBackground icon;
    public StyleBackground Icon => icon;
    [SerializeField] private string diceName;
    public string DiceName => LocalizationManager.Instance.GetLocalizedText(diceName);
    [SerializeField] private int[] values;
    public int[] Values => values;
    public int RandomValue => values[Random.Range(0, values.Length)];
    [Range(0.1f, 1f)][SerializeField] private float boardScale;
    [Range(0.1f, 1f)][SerializeField] private float previewScale;
    [SerializeField] private float boardRotationSpeed;
    [SerializeField] private float previewRotationSpeed;
    [SerializeField] private float boardTiltAmplitude;
    [SerializeField] private float previewTiltAmplitude;
    [SerializeField] private float boardTiltFrequency;
    [SerializeField] private float previewTiltFrequency;
    [SerializeField] private float numberAnimationSpeed;
    [SerializeField] private TextMeshPro[] numberLabels;
    [SerializeField] private TextMeshPro resultLabel;
    public string ResultLabel(string value) => resultLabel.text = value;
    [SerializeField] private AnimationCurve scaleEase;

    private float tiltTime = 0f;

    private float scale;
    private float rotationSpeed;
    private float tiltAmplitude;
    private float tiltFrequency;
    private bool isSpinning = true;
    public bool IsSpinning {
        get => isSpinning;
        set => isSpinning = value;
    }
    private bool inPreview = true;
    public bool SetInPreview {
        set {
            inPreview = value;
            rotationSpeed = inPreview ? previewRotationSpeed : boardRotationSpeed;
            tiltAmplitude = inPreview ? previewTiltAmplitude : boardTiltAmplitude;
            tiltFrequency = inPreview ? previewTiltFrequency : boardTiltFrequency;
            scale = inPreview ? previewScale : boardScale;
            model.transform.localScale = Vector3.one * scale;
        }
    }

    private void Start() {
        SetInPreview = true;
        icon = new StyleBackground(iconTexture2D);
    }

    void Update() {
        if (isSpinning) { Spin(); return; }
        if (resultLabel.gameObject.activeSelf) {
            resultLabel.transform.rotation = Quaternion.LookRotation(resultLabel.transform.position - Camera.main.transform.position);
        }
    }

    public void StopSpinning() {
        isSpinning = false;
        tiltTime = 0f;
        model.transform.rotation = Quaternion.Euler(0, model.transform.rotation.eulerAngles.y, 0);
    }

    public void Show() {
        model.gameObject.SetActive(true);
    }

    public void Hide() {
        HideModel();
        HideDiceResultLabel();
    }

    public void HideModel() {
        model.transform.localScale = Vector3.one * scale;
        model.gameObject.SetActive(false);
        StopSpinning();
    }

    public void OnRollStart() {
        isSpinning = true;
        StartCoroutine(RandomDiceNumberCoroutine());

        Show();
        model.transform.DOScale(0, .3f).From();

        IEnumerator RandomDiceNumberCoroutine() {
            if (!isSpinning) { yield break; }

            var num = Random.Range(0, values.Length);
            SetDiceNumber(values[num]);
            yield return new WaitForSeconds(numberAnimationSpeed);
            StartCoroutine(RandomDiceNumberCoroutine());
        }
    }

    public void OnRollCancel() {
        StopSpinning();
        model.transform.DOComplete();
        model.transform.DOScale(0, .12f).OnComplete(() => HideModel());
    }

    public void OnRollDisplay(int roll) {
        hitParticle.Play();
        Audiomanager.Instance?.PlayDiceStopSound();
        if (hitParticle.GetComponent<CinemachineImpulseSource>() != null) {
            hitParticle.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        }
        model.transform.DOComplete();
        StopSpinning();
        SetDiceNumber(roll);
        model.transform.eulerAngles = Vector3.zero;
        var localPosition = model.transform.localPosition;
        model.transform.DOLocalJump(localPosition, .8f, 1, .25f);
        model.transform.DOPunchScale(Vector3.one / 4, .3f, 10, 1);
    }

    public WaitWhile OnRollEnd(int roll) {
        Hide();
        resultParticle.Play();

        ShowDiceResultLabel();
        resultLabel.text = roll.ToString();
        resultLabel.transform.DOComplete();
        resultLabel.transform.DOScale(0, .2f).From().SetEase(scaleEase);

        return new WaitWhile(() => resultParticle.isPlaying);
    }

    public void ShowDiceResultLabel() {
        resultLabel.gameObject.SetActive(true);
    }

    public void HideDiceResultLabel() {
        resultLabel.gameObject.SetActive(false);
    }

    private void Spin() {
        model.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        tiltTime += Time.deltaTime * tiltFrequency;
        var tiltAngle = Mathf.Sin(tiltTime) * tiltAmplitude;

        model.transform.rotation = Quaternion.Euler(tiltAngle, model.transform.rotation.eulerAngles.y, 0);
    }

    private void SetDiceNumber(int value) {
        foreach (var label in numberLabels) {
            label.text = value.ToString();
        }
    }
}