using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelectionUI : MonoBehaviour {
    [SerializeField] private UIDocument selectionUI;
    private Button previousDiceBtn;
    public Button PreviousDiceBtn => previousDiceBtn;
    private Button nextDiceBtn;
    public Button NextDiceBtn => nextDiceBtn;
    private Label diceNameLabel;
    public Label DiceNameLabel => diceNameLabel;
    private Label diceInfoLabel;
    public Label DiceInfoLabel => diceInfoLabel;
    private Button previousCharacterBtn;
    public Button PreviousCharacterBtn => previousCharacterBtn;
    private Button nextCharacterBtn;
    public Button NextCharacterBtn => nextCharacterBtn;
    private Label characterNameLabel;
    public Label CharacterNameLabel => characterNameLabel;
    private Label characterInfoLabel;
    public Label CharacterInfoLabel => characterInfoLabel;
    private TextField playerNameInput;
    public TextField PlayerNameInput => playerNameInput;
    private Button confirmChoiceBtn;
    public Button ConfirmChoiceBtn => confirmChoiceBtn;

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
        confirmChoiceBtn = root.Q<Button>("SelectButton");
    }
}