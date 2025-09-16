using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsHistoryModal : NewModal {
    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsHistoryModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
    }
}