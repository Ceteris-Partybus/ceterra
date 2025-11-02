using DG.Tweening;
using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour {
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite hiddenIconSprite;
    [SerializeField] private float flipDuration = 0.3f;
    private Sprite iconSprite;
    public Sprite GetIconSprite => iconSprite;

    public MgMemoryGameController memoryController;

    private bool isSelected;
    private bool isAnimating;

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
        if (isAnimating) {
            return;
        }

        StartFlipAnimation(iconSprite, true);
    }

    public void Hide() {
        if (isAnimating) {
            return;
        }

        StartFlipAnimation(hiddenIconSprite, false);
    }

    private void StartFlipAnimation(Sprite targetSprite, bool willBeSelected) {
        isAnimating = true;

        cardImage.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                cardImage.sprite = targetSprite;
                isSelected = willBeSelected;

                cardImage.transform.DORotate(Vector3.zero, flipDuration / 2f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                    });
            });
    }
}