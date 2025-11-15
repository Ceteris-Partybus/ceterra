using System;

public class CoinLossParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action GetDestroySoundEmitter() => Audiomanager.Instance.PlayCoinLossSound;
}