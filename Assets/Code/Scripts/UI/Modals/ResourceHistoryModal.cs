using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceHistoryModal : Modal {
    public static ResourceHistoryModal Instance => GetInstance<ResourceHistoryModal>();
    protected override string GetHeaderTitle() {
        return LocalizationManager.Instance.GetLocalizedText(56156500152803328);
    }
    [SerializeField]
    private VisualTreeAsset entryTemplate;

    private ScrollView scrollView;
    private VisualElement container;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.ResourceHistoryModalTemplate;
        showModalTypeInHeader = true;
        base.Start();
    }

    protected override void OnModalShown() {
        this.scrollView = modalBackground.Q<ScrollView>();
        this.container = scrollView.contentContainer;

        List<ResourceHistoryEntry> history = BoardContext.Instance.resourceHistory.ToList();
        history.Reverse();

        foreach (var entry in history) {
            AddEntry(entry);
        }
    }

    private VisualElement CreateEntryCard(ResourceHistoryEntry entry) {
        VisualElement entryCard = entryTemplate.Instantiate();
        entryCard.name = $"resource-history-entry-{entry.GetHashCode()}";
        entryCard.Q<Label>("entry-source").text = entry.source;
        char sign = entry.type == HistoryEntryType.DEPOSIT ? '+' : '-';
        entryCard.Q<Label>("entry-amount").text = $"{sign}{entry.amount}";
        
        string colorClass = entry.type == HistoryEntryType.DEPOSIT ? "history-entry-amount-deposit" : "history-entry-amount-withdraw";
        entryCard.Q<Label>("entry-amount").AddToClassList(colorClass);

        return entryCard;
    }

    private void AddEntry(ResourceHistoryEntry entry) {
        var entryElement = CreateEntryCard(entry);
        container.Insert(container.childCount, entryElement);
    }

    public void AddEntryToTop(ResourceHistoryEntry entry) {
        var entryElement = CreateEntryCard(entry);
        container.Insert(0, entryElement);
    }
}