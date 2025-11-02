using DG.Tweening;
using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour {
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite hiddenIconSprite;
    [SerializeField] private float flipDuration = 0.3f; // Dauer der Flip-Animation
    private Sprite iconSprite;
    public Sprite GetIconSprite => iconSprite;

    public MgMemoryGameController memoryController;
    
    [SyncVar] private int cardIndex = -1; // Eindeutige ID für Netzwerk-Synchronisation
    public int CardIndex => cardIndex;

    private bool isSelected;
    private bool isAnimating; // Verhindert mehrfache Animationen

    public bool IsSelected => isSelected;

    private void Awake() {
        cardImage ??= GetComponent<Image>();
        Hide();
    }

    public void OnCardClicked() {
        // Command direkt an GameController senden
        memoryController.CmdSelectCard(cardIndex);
    }

    public void SetCardIndex(int index) {
        cardIndex = index;
    }

    public void SetIconSprite(Sprite sprite) {
        iconSprite = sprite;
    }

    // Lokale Show-Methode ohne Netzwerk (wird über RPC aufgerufen)
    public void ShowLocal() {
        if (isAnimating) {
            return;
        }
        
        StartFlipAnimation(iconSprite, true);
    }

    // Lokale Hide-Methode ohne Netzwerk (wird über RPC aufgerufen)
    public void HideLocal() {
        if (isAnimating) {
            return;
        }
        
        StartFlipAnimation(hiddenIconSprite, false);
    }

    // Deprecated - nur für Kompatibilität
    public void Show() {
        ShowLocal();
    }

    // Deprecated - nur für Kompatibilität  
    public void Hide() {
        HideLocal();
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