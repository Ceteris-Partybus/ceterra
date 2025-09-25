using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private Transform particles;
    public Transform Particles => particles;
    [SerializeField] private ParticleSystem coinGainParticle;
    public ParticleSystem CoinGainParticle => coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;
    public ParticleSystem CoinLossParticle => coinLossParticle;
    [SerializeField] private ParticleSystem healthGainParticle;
    public ParticleSystem HealthGainParticle => healthGainParticle;
    [SerializeField] private ParticleSystem healthLossParticle;
    public ParticleSystem HealthLossParticle => healthLossParticle;

    [Header("Player Parameters")]
    [SerializeField] private Transform playerModel;

    [Header("General")]
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private string characterName;
    public string CharacterName => characterName;
    [SerializeField] private string info;
    public string Info => info;
    [SerializeField] private int jumpPower;
    [SerializeField] private float jumpDuration;

    public void Jump() {
        playerModel.DOComplete();
        playerModel.DOJump(transform.position, jumpPower, 1, jumpDuration);
    }

    public void FaceCamera() {
        var directionToCamera = Camera.main.transform.position - playerModel.transform.position;
        directionToCamera.y = 0;
        if (directionToCamera.sqrMagnitude > 0.0001f) {
            var targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            playerModel.rotation = Quaternion.Lerp(playerModel.transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    public void SetMovementRotation(Quaternion targetRotation, float lerpSpeed) {
        playerModel.rotation = Quaternion.Lerp(playerModel.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }
}