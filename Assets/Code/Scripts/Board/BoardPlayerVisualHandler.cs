using Mirror;
using UnityEngine;

public class BoardPlayerVisualHandler : NetworkBehaviour {
    [Header("Particles")]
    [SerializeField] private ParticleSystem coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;

    public void PlayCoinGainParticle() {
        coinGainParticle.Play();
    }

    public void PlayCoinLossParticle() {
        coinLossParticle.Play();
    }
}