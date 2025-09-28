using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class CharacterSelectionController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private CinemachineCamera characterSelectionCamera;
    [SerializeField] private CharacterSelectionUI characterSelectionUI;
    private string playerName;
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

    public Action OnRequestBackToLobby;

    void Start() {
        var characterCount = GameManager.Singleton.CharacterCount;
        characterInstances = new GameObject[characterCount];
        for (var i = 0; i < characterCount; i++) {
            characterInstances[i] = Instantiate(GameManager.Singleton.GetCharacter(i), characterPosition.position, Quaternion.identity, characterPosition);
            var targetPosition = characterSelectionCamera.transform.position;
            targetPosition.y = characterInstances[i].transform.position.y;
            characterInstances[i].transform.LookAt(targetPosition);
            characterInstances[i].SetActive(i == 0);
        }
        var diceCount = GameManager.Singleton.DiceCount;
        diceInstances = new GameObject[diceCount];
        for (var i = 0; i < diceCount; i++) {
            diceInstances[i] = Instantiate(GameManager.Singleton.GetDice(i), dicePosition.position, Quaternion.identity, dicePosition);
            diceInstances[i].SetActive(i == 0);
        }
        currentDice = diceInstances[0];
        currentCharacter = characterInstances[0];

        setListeners();
    }

    private void setListeners() {
        characterSelectionUI.previousCharacterBtn.clicked += OnPreviousCharacter;
        characterSelectionUI.previousCharacterBtn.clicked += OnCharacterChanged;

        characterSelectionUI.nextCharacterBtn.clicked += OnNextCharacter;
        characterSelectionUI.nextCharacterBtn.clicked += OnCharacterChanged;

        characterSelectionUI.previousDiceBtn.clicked += OnPreviousDice;
        characterSelectionUI.previousDiceBtn.clicked += OnDiceChanged;

        characterSelectionUI.nextDiceBtn.clicked += OnNextDice;
        characterSelectionUI.nextDiceBtn.clicked += OnDiceChanged;

        characterSelectionUI.confirmSelectionBtn.clicked += OnSelectionConfirmed;

        characterSelectionUI.playerNameInput.RegisterValueChangedCallback(e => OnPlayerNameChanged(e.newValue));
        characterSelectionUI.playerNameInput.value = playerName;

        OnCharacterChanged();
        OnDiceChanged();
    }

    private void OnPlayerNameChanged(string newValue) {
        playerName = Regex.Replace(newValue ?? "", @"\s+", " ");
        characterSelectionUI.playerNameInput.SetValueWithoutNotify(playerName);
        characterSelectionUI.confirmSelectionBtn.SetEnabled(!string.IsNullOrWhiteSpace(playerName));
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

    private void OnSelectionConfirmed() {
        playerName = playerName.Trim();
        GameManager.Singleton.roomSlots
             .OfType<LobbyPlayer>()
             .FirstOrDefault(player => player.isLocalPlayer)
             .CmdSetCharacterSelection(currentCharacterIndex, currentDiceIndex, playerName);
        OnRequestBackToLobby?.Invoke();
    }

    private void OnDiceChanged() {
        var dice = CurrentDice;
        characterSelectionUI.diceNameLabel.text = dice.DiceName;
        characterSelectionUI.diceInfoLabel.text = string.Join(", ", dice.Values);
    }

    private void OnCharacterChanged() {
        var character = CurrentCharacter;
        characterSelectionUI.characterNameLabel.text = character.CharacterName;
        characterSelectionUI.characterInfoLabel.text = character.Info;
    }

    public void ToggleCharacterSelection() {
        selectionUI.gameObject.SetActive(!selectionUI.gameObject.activeSelf);
        if (selectionUI.gameObject.activeSelf) {
            setListeners();
        }
    }
}