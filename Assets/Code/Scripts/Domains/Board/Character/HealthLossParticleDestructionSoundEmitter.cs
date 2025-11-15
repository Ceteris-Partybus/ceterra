using System;

public class HealthLossParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action GetDestroySoundEmitter() => Audiomanager.Instance.PlayHealthLossSound;
}