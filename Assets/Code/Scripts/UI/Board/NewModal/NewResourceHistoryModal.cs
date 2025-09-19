public class NewResourceHistoryModal : NewModal {

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ResourceHistoryModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
    }
}