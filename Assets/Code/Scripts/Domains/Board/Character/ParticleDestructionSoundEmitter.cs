using System;
using UnityEngine;

public abstract class ParticleDestructionSoundEmitter : MonoBehaviour {
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private BoardPlayer boardPlayer;
    protected abstract Action<BoardPlayer> GetDestroySoundEmitter();
    private int currentParticleCount = 0;

    void Update() {
        if (_particleSystem.particleCount < currentParticleCount) {
            Debug.Log("Playing particle destruction sound");
            GetDestroySoundEmitter()?.Invoke(boardPlayer);
        }
        currentParticleCount = _particleSystem.particleCount;
    }
}