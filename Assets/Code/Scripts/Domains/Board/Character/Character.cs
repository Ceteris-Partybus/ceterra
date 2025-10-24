using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class Character : MonoBehaviour {
    [Header("Player Parameters")]
    private Animator animator;
    public Animator Animator => animator ??= GetComponent<Animator>();
    [SerializeField] private Texture2D iconTexture2D;
    private StyleBackground icon;
    public StyleBackground Icon => icon;

    [Header("General")]
    [SerializeField] private string characterName;
    public string CharacterName {
        get => LocalizedCharacterName();
        set => characterName = value;
    }
    public string BaseCharacterName => characterName;

    [SerializeField] private string info;
    public string Info => LocalizationManager.Instance.GetLocalizedText(info);
    private const float JUMP_POWER = 1.5f;
    private const float JUMP_DURATION = .6f;

    void Start() {
        icon = new StyleBackground(iconTexture2D);
    }

    public void HitDice() {
        transform.DOComplete();
        transform.DOJump(transform.position, JUMP_POWER, 1, JUMP_DURATION);
    }

    public void FaceCamera() {
        var directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0;
        if (directionToCamera.sqrMagnitude > .0001f) {
            var targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    public void SetMovementRotation(Quaternion targetRotation, float lerpSpeed) {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }

    private string LocalizedCharacterName() {
        var data = characterName.Split(" ");
        return LocalizationManager.Instance.GetLocalizedText(data[0]) + " " + data[1];
    }
}