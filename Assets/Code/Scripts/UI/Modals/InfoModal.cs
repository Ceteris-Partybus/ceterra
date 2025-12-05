using UnityEngine.UIElements;

public class InfoModal : Modal {
    public static InfoModal Instance => GetInstance<InfoModal>();
    protected override string GetHeaderTitle() {
        return "Info";
    }
    public string Message = "";
    Label infoMessageLabel;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.InfoModalTemplate;
        showModalTypeInHeader = true;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        infoMessageLabel = modalElement.Q<Label>("info-message");
        if (infoMessageLabel != null && !string.IsNullOrEmpty(Message)) {
            infoMessageLabel.text = Message;
        }
        modalBoxWrapper.Q<Label>("modal-header-title").AddToClassList("info-modal-title");
    }
}