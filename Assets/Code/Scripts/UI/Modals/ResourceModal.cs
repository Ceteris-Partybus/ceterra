using Mirror;
using UnityEngine.UIElements;

public class ResourceModal : Modal {
    public static ResourceModal Instance => GetInstance<ResourceModal>();
    protected override string GetHeaderTitle() {
        return LocalizationManager.Instance.GetLocalizedText(56155672947974144);
    }
    private Label resourceNextRoundLabel;

    private Button resourcesHistoryButton;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ResourceModalTemplate;
        showModalTypeInHeader = true;
        base.Start();
    }

    protected override void OnModalShown() {
        this.resourcesHistoryButton = modalElement.Q<Button>("resources-history-button");
        this.resourceNextRoundLabel = modalElement.Q<Label>("resources-next-round");
        this.resourcesHistoryButton.clicked += this.OnResourcesHistoryButtonClicked;
        Refresh();
    }

    [ClientCallback]
    public void Refresh() {
        resourceNextRoundLabel.text = BoardContext.Instance.ResourcesNextRound.ToString();
    }

    private void OnResourcesHistoryButtonClicked() {
        Audiomanager.Instance?.PlayClickSound();
        ModalManager.Instance.Show(ResourceHistoryModal.Instance);
    }
}