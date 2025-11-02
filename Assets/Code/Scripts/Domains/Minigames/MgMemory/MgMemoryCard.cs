using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour {
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite hiddenIconSprite;
    private Sprite iconSprite;
    public Sprite GetIconSprite => iconSprite;

    public MgMemoryGameController memoryController;

    private bool isSelected;

    public bool IsSelected => isSelected;

    private void Awake() {
        cardImage ??= GetComponent<Image>();
        Hide();
    }

    public void OnCardClicked() {
        memoryController.SetSelectedCard(this);
    }

    public void SetIconSprite(Sprite sprite) {
        iconSprite = sprite;
    }

    public void Show() {
        cardImage.sprite = iconSprite;
        isSelected = true;
    }

    public void Hide() {
        cardImage.sprite = hiddenIconSprite;
        isSelected = false;
    }
}