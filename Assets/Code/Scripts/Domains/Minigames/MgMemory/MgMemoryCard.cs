using DG.Tweening;
using Mirror;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour {
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite hiddenIconSprite;
    [SerializeField] private float flipDuration = 0.3f;
    private Sprite iconSprite;
    public Sprite GetIconSprite => iconSprite;

    public MgMemoryGameController memoryController;

    private int cardIndex = -1;
    public int CardIndex => cardIndex;

    private bool isSelected;
    private bool isAnimating;

    public bool IsSelected => isSelected;

    private void Awake() {
        cardImage ??= GetComponent<Image>();
        Hide();
    }

    public void OnCardClicked() {
        memoryController.CmdSelectCard(cardIndex, GetLocalPlayer());
    }

    public void SetCardIndex(int index) {
        cardIndex = index;
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

    public MgMemoryPlayer GetLocalPlayer() {
        return FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
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