using System;

public class CoinLossParticleDestructionSoundEmitter : ParticleDestructionSoundEmitter {
    protected override Action<BoardPlayer> GetDestroySoundEmitter() => Audiomanager.Instance.PlayCoinLossSound;
}