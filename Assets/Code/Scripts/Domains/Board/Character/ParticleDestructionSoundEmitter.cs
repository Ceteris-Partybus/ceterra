using System;
using UnityEngine;

public abstract class ParticleDestructionSoundEmitter : MonoBehaviour {
    [SerializeField] private ParticleSystem _particleSystem;
    protected abstract Action GetDestroySoundEmitter();
    private int currentParticleCount = 0;

    void Update() {
        if (_particleSystem.particleCount < currentParticleCount) {
            GetDestroySoundEmitter()?.Invoke();
        }
        currentParticleCount = _particleSystem.particleCount;
    }
}