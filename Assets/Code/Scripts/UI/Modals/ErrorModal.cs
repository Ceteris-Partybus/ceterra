using UnityEngine.UIElements;

public class ErrorModal : Modal {
    public static ErrorModal Instance => GetInstance<ErrorModal>();
    protected override string GetHeaderTitle() {
        return LocalizationManager.Instance.GetLocalizedText(56147343727124480);
    }
    public string Message;
    Label errorMessageLabel;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ErrorModalTemplate;
        showModalTypeInHeader = true;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        errorMessageLabel = modalElement.Q<Label>("error-message");
        if (errorMessageLabel != null && !string.IsNullOrEmpty(Message)) {
            errorMessageLabel.text = Message;
        }
        modalBoxWrapper.Q<Label>("modal-header-title").AddToClassList("error-modal-title");
    }
}