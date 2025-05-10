using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DialogueHandler : MonoBehaviour {

    private UIDocument uiDocument;
    private VisualElement currentDialogue;

    [SerializeField]
    private bool allowMultipleDialogues = true;
    [SerializeField]
    private List<string> elementNames = new();

    [SerializeField]
    private List<Dialogue> dialogues = new();

    void Start() {
        if (elementNames.Count != dialogues.Count) {
            Debug.LogError("Element names and dialogues count mismatch");
            return;
        }

        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null) {
            Debug.LogError("UIDocument is null");
            return;
        }

        for (int i = 0; i < elementNames.Count; i++) {
            string elementName = elementNames[i];
            Dialogue dialogue = dialogues[i];

            var element = uiDocument.rootVisualElement.Q(elementName);
            if (element != null) {
                element.RegisterCallback<ClickEvent>(e => OpenDialogue(dialogue));
            }
            else {
                Debug.LogError($"Element with name {elementName} not found");
            }
        }
    }

    void OpenDialogue(Dialogue dialogue) {
        if (!allowMultipleDialogues) {
            currentDialogue?.RemoveFromHierarchy();
        }

        VisualTreeAsset dialogueTemplate = Resources.Load<VisualTreeAsset>("Dialogue");
        VisualElement dialogueContainer = dialogueTemplate.Instantiate();
        dialogueContainer.dataSource = dialogue;
        uiDocument.rootVisualElement.parent.Add(dialogueContainer);

        if (dialogueContainer == null) {
            Debug.LogError("Dialogue container is null");
            return;
        }

        var closeButton = GetCloseButton(dialogueContainer);
        if (closeButton == null) {
            Debug.LogError("Close button is null");
            return;
        }

        var dialogueContentContainer = dialogueContainer.Q("dialogue__content");
        if (dialogueContentContainer == null) {
            Debug.LogError("Dialogue content is null");
            return;
        }

        var dialogueContent = dialogue.content.Instantiate();
        dialogueContentContainer.Add(dialogueContent);

        currentDialogue = dialogueContainer;

        closeButton.RegisterCallback<ClickEvent>(e => {
            currentDialogue?.RemoveFromHierarchy();
        });
    }

    private UnityEngine.UIElements.Button GetCloseButton(VisualElement dialogueContainer) {
        var closeButton = dialogueContainer.Q<UnityEngine.UIElements.Button>("dialogue__close");
        return closeButton;
    }
}
