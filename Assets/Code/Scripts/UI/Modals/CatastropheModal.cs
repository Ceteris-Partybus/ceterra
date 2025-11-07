using UnityEngine.UIElements;

public class CatastropheModal : Modal {
    public static CatastropheModal Instance => GetInstance<CatastropheModal>();
    public string Title = "";
    public string Description = "";
    public string AffectedPlayers = "";
    Label titleLabel;
    Label descriptionLabel;
    Label affectedPlayersLabel;

    protected override void Start() {
        this.closeOnBackgroundClick = false;
        this.closeOnEscapeKey = false;
        this.showCloseButton = false;
        this.showModalTypeInHeader = true;
        this.visualTreeAsset = ModalMap.Instance.CatastropheModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        titleLabel = modalElement.Q<Label>("catastrophe-title");
        descriptionLabel = modalElement.Q<Label>("catastrophe-description");
        affectedPlayersLabel = modalElement.Q<Label>("affected-players");

        titleLabel.text = Title;
        descriptionLabel.text = Description;
        affectedPlayersLabel.text = AffectedPlayers;
    }

    protected override string GetHeaderTitle() {
        return LocalizationManager.Instance.GetLocalizedText(56636904074215424);
    }
}