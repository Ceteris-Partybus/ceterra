using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class Character : MonoBehaviour {
    [Header("Player Parameters")]
    [SerializeField] private Transform model;
    [SerializeField] private Texture2D iconTexture2D;
    private StyleBackground icon;
    public StyleBackground Icon => icon;

    [Header("General")]
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private string characterName;
    public string CharacterName => characterName;
    [SerializeField] private string info;
    public string Info => info;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpDuration;

    void Start() {
        icon = new StyleBackground(iconTexture2D);
    }

    public void HitDice() {
        model.DOComplete();
        model.DOJump(transform.position, jumpPower, 1, jumpDuration);
    }

    public void FaceCamera() {
        var directionToCamera = Camera.main.transform.position - model.transform.position;
        directionToCamera.y = 0;
        if (directionToCamera.sqrMagnitude > 0.0001f) {
            var targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            model.rotation = Quaternion.Lerp(model.transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    public void SetMovementRotation(Quaternion targetRotation, float lerpSpeed) {
        model.rotation = Quaternion.Lerp(model.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }
}