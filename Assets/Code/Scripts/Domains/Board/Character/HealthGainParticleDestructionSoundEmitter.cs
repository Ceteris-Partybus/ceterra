using System;

public class HealthGainParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action<BoardPlayer> GetDestroySoundEmitter() => Audiomanager.Instance.PlayHealthGainSound;
}