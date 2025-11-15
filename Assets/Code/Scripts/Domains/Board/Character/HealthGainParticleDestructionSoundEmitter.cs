using System;

public class HealthGainParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action GetDestroySoundEmitter() => Audiomanager.Instance.PlayHealthGainSound;
}