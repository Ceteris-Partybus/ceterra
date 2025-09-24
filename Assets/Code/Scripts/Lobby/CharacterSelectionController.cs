using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CharacterSelectionController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject selectionUi;
    [SerializeField] private CinemachineCamera characterSelectionCamera;
    [SerializeField] private CharacterSelectionUI characterSelectionUI;

    private GameObject[] characterInstances;
    private int currentCharacterIndex = 0;
    private GameObject currentCharacter;
    public Character CurrentCharacter => currentCharacter.GetComponent<Character>();
    private GameObject[] diceInstances;
    private int currentDiceIndex = 0;
    private GameObject currentDice;
    public Dice CurrentDice => currentDice.GetComponent<Dice>();

    [Header("Positions")]
    [SerializeField] private Transform characterPosition;
    [SerializeField] private Transform dicePosition;

    void Start() {
        var characterCount = GameManager.Singleton.CharacterCount;
        characterInstances = new GameObject[characterCount];
        for (int i = 0; i < characterCount; i++) {
            characterInstances[i] = Instantiate(GameManager.Singleton.GetCharacter(i), characterPosition.position, Quaternion.identity, characterPosition);
            var targetPosition = characterSelectionCamera.transform.position;
            targetPosition.y = characterInstances[i].transform.position.y;
            characterInstances[i].transform.LookAt(targetPosition);
            characterInstances[i].SetActive(i == 0);
        }
        var diceCount = GameManager.Singleton.DiceCount;
        diceInstances = new GameObject[diceCount];
        for (int i = 0; i < diceCount; i++) {
            diceInstances[i] = Instantiate(GameManager.Singleton.GetDice(i), dicePosition.position, Quaternion.identity, dicePosition);
            diceInstances[i].SetActive(i == 0);
        }
        currentDice = diceInstances[0];
        currentCharacter = characterInstances[0];

        setListeners();
    }

    private void setListeners() {
        characterSelectionUI.PreviousCharacterBtn.clicked += OnPreviousCharacter;
        characterSelectionUI.PreviousCharacterBtn.clicked += UpdateCharacterInfo;

        characterSelectionUI.NextCharacterBtn.clicked += OnNextCharacter;
        characterSelectionUI.NextCharacterBtn.clicked += UpdateCharacterInfo;

        characterSelectionUI.PreviousDiceBtn.clicked += OnPreviousDice;
        characterSelectionUI.PreviousDiceBtn.clicked += UpdateDiceInfo;

        characterSelectionUI.NextDiceBtn.clicked += OnNextDice;
        characterSelectionUI.NextDiceBtn.clicked += UpdateDiceInfo;

        characterSelectionUI.ConfirmChoiceBtn.clicked += OnConfirmChoice;
        UpdateCharacterInfo();
        UpdateDiceInfo();
    }

    private void OnNextDice() {
        ChangeDice((currentDiceIndex + 1) % diceInstances.Length);
    }

    private void OnPreviousDice() {
        ChangeDice((currentDiceIndex - 1 + diceInstances.Length) % diceInstances.Length);
    }

    private void ChangeDice(int newIndex) {
        currentDice.SetActive(false);
        currentDiceIndex = newIndex;
        currentDice = diceInstances[currentDiceIndex];
        currentDice.SetActive(true);
    }

    private void OnNextCharacter() {
        ChangeCharacter((currentCharacterIndex + 1) % characterInstances.Length);
    }

    private void OnPreviousCharacter() {
        ChangeCharacter((currentCharacterIndex - 1 + characterInstances.Length) % characterInstances.Length);
    }

    private void ChangeCharacter(int newIndex) {
        currentCharacter.SetActive(false);
        currentCharacterIndex = newIndex;
        currentCharacter = characterInstances[currentCharacterIndex];
        currentCharacter.SetActive(true);
    }

    private void OnConfirmChoice() {
        GameManager.Singleton.roomSlots
             .OfType<LobbyPlayer>()
             .FirstOrDefault(player => player.isLocalPlayer)
             .CmdSetCharacterSelection(currentCharacterIndex, currentDiceIndex);
    }

    private void UpdateDiceInfo() {
        var dice = CurrentDice;
        characterSelectionUI.DiceNameLabel.text = dice.DiceName;
        characterSelectionUI.DiceInfoLabel.text = string.Join(", ", dice.Values);
    }

    private void UpdateCharacterInfo() {
        var character = CurrentCharacter;
        characterSelectionUI.CharacterNameLabel.text = character.CharacterName;
        characterSelectionUI.CharacterInfoLabel.text = character.Info;
    }

    public void ToggleCharacterSelection() {
        selectionUi.gameObject.SetActive(!selectionUi.gameObject.activeSelf);
        if (selectionUi.gameObject.activeSelf) {
            setListeners();
        }
    }
}