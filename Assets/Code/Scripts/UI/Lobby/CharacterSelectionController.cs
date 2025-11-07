using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class CharacterSelectionController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject selectionUI;
    public GameObject SelectionUI => selectionUI;
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

    private Dictionary<string, Renderer> currentCharacterMaterials = new Dictionary<string, Renderer>();

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

    public void ToggleCharacterSelection() {
        selectionUI.gameObject.SetActive(!selectionUI.gameObject.activeSelf);
        if (selectionUI.gameObject.activeSelf) {
            setListeners();
        }
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

        //Button Sounds
        characterSelectionUI.previousCharacterBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();
        characterSelectionUI.nextCharacterBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();
        characterSelectionUI.previousDiceBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();
        characterSelectionUI.nextDiceBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();
        characterSelectionUI.confirmSelectionBtn.clicked += () => Audiomanager.Instance?.PlayClickSound();

        OnCharacterChanged();
        OnDiceChanged();
    }

    private void OnPlayerNameChanged(string newValue) {
        playerName = Regex.Replace(newValue ?? "", @"\s+", " ");
        playerName = playerName.Length > 20 ? playerName.Substring(0, 20) : playerName;
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

        var smr = currentCharacter.GetComponentInChildren<SkinnedMeshRenderer>();
        var materialColorInfo = smr.materials.Select((mat, index) => new MaterialColorInfo { index = index, color = mat.color }).ToArray();
        GameManager.Singleton.roomSlots
             .OfType<LobbyPlayer>()
             .FirstOrDefault(player => player.isLocalPlayer)
             .CmdSetCharacterSelection(currentCharacterIndex, currentDiceIndex, playerName, materialColorInfo);
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

        BuildCharacterMaterialsUI();
    }

    private void BuildCharacterMaterialsUI() {
        if (!ColorPicker.done) {
            ColorPicker.Cancel();
        }
        currentCharacterMaterials.Clear();
        if (characterSelectionUI.characterMaterialsContainer != null) {
            characterSelectionUI.characterMaterialsContainer.Clear();
        }

        var smr = currentCharacter.GetComponentInChildren<SkinnedMeshRenderer>();
        foreach (var material in smr.materials) {
            var materialRow = new VisualElement();
            materialRow.AddToClassList("material-row");

            material.name = material.name.Replace(" (Instance)", "");
            var label = new Label(GetMaterialTranslation(material.name));
            label.AddToClassList("material-label");

            var colorBox = new Button();
            colorBox.clicked += () => OnMaterialColorPickerClicked(material, colorBox);
            colorBox.style.backgroundColor = material.color;
            colorBox.AddToClassList("material-color-box");

            materialRow.Add(label);
            materialRow.Add(colorBox);

            characterSelectionUI.characterMaterialsContainer.Add(materialRow);
        }
    }

    private void OnMaterialColorPickerClicked(Material material, Button btn) {
        if (!ColorPicker.done) {
            ColorPicker.Cancel();
        }
        ColorPicker.Create(
            material.color,
            $"{LocalizationManager.Instance.GetLocalizedText(59269036665028608)}: {GetMaterialTranslation(material.name)}!",
            (color) => OnColorChanged(material, color, btn),
            (_) => { }
        );
    }

    private void OnColorChanged(Material material, Color newColor, Button btn) {
        material.color = newColor;
        btn.style.backgroundColor = newColor;
    }

    private string GetMaterialTranslation(string materialName) {
        var translationKey = CurrentCharacter.BaseCharacterName.Split("_")[0] + "." + materialName;
        return LocalizationManager.Instance.GetLocalizedText(translationKey);
    }
}