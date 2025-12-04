using System;

public class CoinGainParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action<BoardPlayer> GetDestroySoundEmitter() => Audiomanager.Instance.PlayCoinGainSound;
}