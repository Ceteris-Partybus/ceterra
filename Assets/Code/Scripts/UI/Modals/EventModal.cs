using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EventModal : Modal {

    public static EventModal Instance => GetInstance<EventModal>();

    public string Title = "";
    public string Description = "";
    Label titleLabel;
    Label descriptionLabel;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.EventModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        titleLabel = modalElement.Q<Label>("event-title");
        descriptionLabel = modalElement.Q<Label>("event-description");

        titleLabel.text = Title;
        descriptionLabel.text = Description;
    }
}