# Modals

1. Erstelle ein `GameObject`
2. Füge ein `UIDocument` hinzu. Wähle als `Source Asset` dein `VisualTreeAsset`. Das `VisualTreeAsset` benötigt ein `VisualElement` auf der obersten Ebene mit dem Namen `modal-container`. Binde außerdem das `ModalStyles` Stylesheet ein.
3. Weise dem `GameObject` das `ModalManager` Skript zu und setze die `UIDocument` Referenz
5. Erstelle eine `MonoBehaviour` und weise diese dem `GameObject` zu.

```csharp
public class Example : MonoBehaviour {

    [SerializeField] private UIDocument uiDocument;

    [Header("Modals")]
    [SerializeField] private VisualTreeAsset modalOneTemplate;
    [SerializeField] private VisualTreeAsset modalTwoTemplate;

    // Can any VisualElement that has an onClick event
    private Button modalOneListener;
    private Button modalTwoListener;

    private void OnEnable() {
        // Get the buttons from the UIDocument
        var root = uiDocument.rootVisualElement;
        this.modalOneListener = root.Q<Button>("modal-one-listener");
        this.modalTwoListener = root.Q<Button>("modal-two-listener");

        // Register the callbacks
        this.modalOneListener.clicked += OpenModalOne;
        this.modalTwoListener.clicked += OpenModalTwo;
    }

    private void OpenModalOne() {
      var modal = new ModalOne(this.modalOneTemplate);
      ModalManager.Instance.ShowModal(modal);
    }

    private void OpenModalTwo() {
      var modal = new ModalTwo(this.modalTwoTemplate);
      ModalManager.Instance.ShowModal(modal);
    }
}
```

6. Erstelle die UXML-Datei für das Modal. Und weise sie dem `Example` Skript im Inspektor als `modalOneTemplate` zu.
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" xsi="http://www.w3.org/2001/XMLSchema-instance" engine="UnityEngine.UIElements" editor="UnityEditor.UIElements" noNamespaceSchemaLocation="../../UIElementsSchema/UIElements.xsd" editor-extension-mode="False">
    <ui:VisualElement style="flex-grow: 1;">
        <ui:Label text="Example Modal" class="modal-title" />
        <ui:Label text="This is an example modal. It has a button." class="modal-text" />
        <ui:VisualElement style="flex-direction: row; justify-content: center;">
            <ui:Button text="Confirm Operation" name="btn-confirm" class="modal-button" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

7. Erstelle einen Handler für den Modal-Inhalt, der von `Modal` erbt. Hier definiert du, was passieren soll, wenn mit Elementen im Modal interagiert wird.

```csharp
public class ModalOne : Modal {
    public ModalOne(VisualTreeAsset contentTemplate) : base(contentTemplate) {
    }

    protected override void InitializeContent() {
        Button confirmButton = this.modalContent.Q<Button>("btn-confirm");

        if (confirmButton != null) {
            confirmButton.clicked += OnConfirmButtonClicked;
        }
    }

    private void OnConfirmButtonClicked() {
        Debug.Log("Confirm button was clicked!");
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        Button confirmButton = this.modalContent.Q<Button>("btn-confirm");
        if (confirmButton != null) {
            confirmButton.clicked -= OnConfirmButtonClicked;
        }
    }
}
```