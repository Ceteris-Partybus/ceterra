using UnityEngine.UIElements;

public class ErrorModal : Modal {

    public static ErrorModal Instance => GetInstance<ErrorModal>();

    public string Message = "An error has occurred.";
    Label errorMessageLabel;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ErrorModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        errorMessageLabel = modalElement.Q<Label>("error-message");
        if (errorMessageLabel != null && !string.IsNullOrEmpty(Message)) {
            errorMessageLabel.text = Message;
        }
    }
}