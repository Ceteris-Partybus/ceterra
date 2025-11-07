using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelectionUI : MonoBehaviour {
    [SerializeField] private UIDocument selectionUI;
    public Button previousDiceBtn;
    public Button nextDiceBtn;
    public Label diceNameLabel;
    public Label diceInfoLabel;
    public Button previousCharacterBtn;
    public Button nextCharacterBtn;
    public Label characterNameLabel;
    public Label characterInfoLabel;
    public TextField playerNameInput;
    public Button confirmSelectionBtn;
    public Button characterColorPickerBtn;
    public VisualElement characterMaterialsContainer;

    void OnEnable() {
        var root = selectionUI.rootVisualElement;
        previousDiceBtn = root.Q<Button>("DicePreviousBtn");
        nextDiceBtn = root.Q<Button>("DiceNextBtn");
        diceNameLabel = root.Q<Label>("DiceNameLabel");
        diceInfoLabel = root.Q<Label>("DiceInfoLabel");
        previousCharacterBtn = root.Q<Button>("CharacterPreviousBtn");
        nextCharacterBtn = root.Q<Button>("CharacterNextBtn");
        characterNameLabel = root.Q<Label>("CharacterNameLabel");
        characterInfoLabel = root.Q<Label>("CharacterInfoLabel");
        playerNameInput = root.Q<TextField>("PlayerNameInput");
        confirmSelectionBtn = root.Q<Button>("SelectButton");
        characterColorPickerBtn = root.Q<Button>("CharacterColorPickerBtn");
        characterMaterialsContainer = root.Q<VisualElement>("CharacterMaterialsContainer");
    }
}