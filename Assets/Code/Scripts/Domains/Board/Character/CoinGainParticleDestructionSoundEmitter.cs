using System;

public class CoinGainParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action GetDestroySoundEmitter() => Audiomanager.Instance.PlayCoinGainSound;
}