using System;
using System.Collections;
using UnityEngine;

public class AtomicExplosion : CatastropheEffect {
    private static readonly long[] MODAL_INFO_TRANSLATION_IDS = { 67262900487143424 };

    public AtomicExplosion() : base(1) { }

    public override CatastropheType GetCatastropheType() => CatastropheType.ATOMIC_EXPLOSION;
    protected override Action GetSoundEmitter() => () => Audiomanager.Instance.PlayAtomicExplosionSound(Vector3.zero);
    //TODO: protected override Vector3 GetSoundSourcePosition() => NuclearPlant.transform.position;
    public override long GetEndDescriptionId() => 56143503242027008;
    public override long GetDisplayNameId() => 67262900419664384;
    public override bool IsGlobal() => true;
    protected override int GetCurrentRoundHealthDamage() => 999;
    protected override int GetCurrentRoundEnvironmentDamage() => 100;
    protected override int GetCurrentRoundDamageResources() => 100;
    protected override int GetCurrentRoundDamageEconomy() => 100;
    protected override long GetCurrentRoundModalDescriptionId() => MODAL_INFO_TRANSLATION_IDS[0];

    protected override IEnumerator Start() {
        CameraHandler.Instance.RpcShakeCamera(10f, 15f);
        yield return new WaitForSeconds(2f);
        GameManager.Singleton.StopGameSwitchEndScene();
    }
}