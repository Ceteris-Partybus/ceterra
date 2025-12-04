using System;

public class HealthLossParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action<BoardPlayer> GetDestroySoundEmitter() => Audiomanager.Instance.PlayHealthLossSound;
}