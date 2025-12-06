using DG.Tweening;
using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour {
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private Sprite hiddenIconSprite;
    [SerializeField] private float flipDuration = 0.5f;
    private MemoryFactData factData;
    public MemoryFactData FactData => factData;
    public void SetFactData(string title, string description, string imagePath) {
        factData = new MemoryFactData(title, description, imagePath);
    }
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
        StartFlipAnimation(hiddenIconSprite, false);
    }

    public void OnCardClicked() {
        Audiomanager.Instance?.PlayCardFlipSound();
        if (isSelected || isAnimating) {
            return;
        }

        memoryController.SelectCard(cardIndex, GetLocalPlayer());
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

    public void ShakeAndFlip(bool playSound = true) {
        if (isAnimating) {
            return;
        }
        var sequence = DOTween.Sequence()
            .Join(cardImage.transform.DOShakePosition(.3f, strength: 50f, vibrato: 50));

        if (playSound) {
            sequence.JoinCallback(() => Audiomanager.Instance?.PlayFailSound());
        }

        sequence.OnComplete(() => StartFlipAnimation(hiddenIconSprite, false));
    }

    private void StartFlipAnimation(Sprite targetSprite, bool willBeSelected) {
        isAnimating = true;

        cardImage.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                cardImage.sprite = targetSprite;
                isSelected = willBeSelected;
                SetButtonInteractable(!willBeSelected);

                cardImage.transform.DORotate(Vector3.zero, flipDuration / 2f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                    });
            });
    }

    public void SetButtonInteractable(bool interactable) {
        cardButton.interactable = interactable;
    }

    public MgMemoryPlayer GetLocalPlayer() {
        return FindObjectsByType<MgMemoryPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }
}