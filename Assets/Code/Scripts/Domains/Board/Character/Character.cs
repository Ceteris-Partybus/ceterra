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
        get => characterName; set =>
        characterName = value;
    }

    [SerializeField] private string info;
    public string Info => info;
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private float jumpDuration = .6f;

    void Start() {
        icon = new StyleBackground(iconTexture2D);
    }

    public void HitDice() {
        transform.DOComplete();
        transform.DOJump(transform.position, jumpPower, 1, jumpDuration);
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
}