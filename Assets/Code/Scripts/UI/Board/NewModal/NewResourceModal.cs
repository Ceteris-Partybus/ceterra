using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewResourceModal : NewModal {

    private Button resourcesHistoryButton;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ResourceModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        this.resourcesHistoryButton = modalElement.Q<Button>("resources-history-button");
        this.resourcesHistoryButton.clicked += this.OnResourcesHistoryButtonClicked;
    }

    private void OnResourcesHistoryButtonClicked() {
        Debug.Log("Action button in Modal 1 was clicked!");
        NewModalManager.Instance.Show(FindObjectsByType<NewResourceHistoryModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault());
    }

}