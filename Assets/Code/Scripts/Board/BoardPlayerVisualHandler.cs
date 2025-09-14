using UnityEngine;
using System.Collections;

public class BoardPlayerVisualHandler : MonoBehaviour {
    [Header("Particles")]
    [SerializeField] private ParticleSystem coinGainParticle;
    [SerializeField] private ParticleSystem coinLossParticle;

    public IEnumerator PlayCoinGainParticle() {
        coinGainParticle.Play();
        yield return new WaitWhile(() => coinGainParticle.isPlaying);
    }

    public IEnumerator PlayCoinLossParticle() {
        coinLossParticle.Play();
        yield return new WaitWhile(() => coinLossParticle.isPlaying);
    }
}