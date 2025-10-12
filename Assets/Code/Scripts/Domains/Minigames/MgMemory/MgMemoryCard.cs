using Mirror;
using UnityEngine;

public class Card : NetworkBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;
    private void Awake() {
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        Hide();
    }

    public void OnCardClicked() {
        Debug.Log($"Card clicked! IsSelected: {isSelected}, IconSprite: {iconSprite?.name}");

        if (isSelected) {
            Hide();
        }
        else {
            Show();
        }
    }

    public void SetIconSprite(Sprite sprite) {
        iconSprite = sprite;
    }

    public void Show() {
        spriteRenderer.sprite = iconSprite;
        isSelected = true;
    }

    public void Hide() {
        spriteRenderer.sprite = hiddenIconSprite;
        isSelected = false;
    }
}