using DG.Tweening;
using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CatastropheFieldBehaviour : FieldBehaviour {
    public override FieldType GetFieldType() => FieldType.CATASTROPHE;
    [SerializeField] private CatastropheType catastropheType;
    public CatastropheType CatastropheType => catastropheType;

    [Server]
    protected override IEnumerator OnPlayerLand(BoardPlayer player) {
        yield return CatastropheManager.Instance.RegisterCatastrophe(catastropheType);

        var localScale = transform.localScale;
        var scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(Vector3.zero, .5f))
                     .AppendCallback(() => RpcChangeMaterial())
                     .Append(transform.DOScale(localScale, .5f));
        yield return scaleSequence.WaitForCompletion();
        yield return new WaitForSeconds(1f);

        FieldInstantiate.Instance.ReplaceField(this, FieldType.NORMAL);
    }

    [ClientRpc]
    private void RpcChangeMaterial() {
        GetComponent<Renderer>().sharedMaterials = FieldInstantiate.Instance.NormalFieldMaterial;
    }
}