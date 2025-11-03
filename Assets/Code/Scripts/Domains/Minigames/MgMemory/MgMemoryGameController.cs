using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MgMemoryGameController : NetworkedSingleton<MgMemoryGameController> {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private readonly float checkDelay = 0.5f;
    [SerializeField] Sprite[] sprites;
    [SerializeField] private int pointsForCorrectMatch = 1;

    private List<Sprite> spritePairs;

    private Card firstSelectedCard;
    private Card secondSelectedCard;

    protected override void Start() {
        base.Start();

        spritePairs = new();
    }

    public void PrepareSprites() {
        spritePairs = new();
        for (var i = 0; i < sprites.Length; i++) {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        ShuffleSprites();
    }

    private void ShuffleSprites() {
        for (var i = 0; i < spritePairs.Count; i++) {
            var temp = spritePairs[i];
            var randomIndex = Random.Range(i, spritePairs.Count);
            spritePairs[i] = spritePairs[randomIndex];
            spritePairs[randomIndex] = temp;
        }
    }

    public void InitializeCardsOnClients(int randomSeed) {
        Random.InitState(randomSeed);
        PrepareSprites();

        InitializeCards();
    }

    void InitializeCards() {
        for (var i = 0; i < spritePairs.Count; i++) {
            var card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.memoryController = this;
            card.SetCardIndex(i);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSelectCard(int cardIndex, MgMemoryPlayer currentPlayer) {
        var selectedCard = FindCardByIndex(cardIndex);
        if (selectedCard == null || selectedCard.IsSelected) {
            return;
        }

        HandleCardSelection(selectedCard, currentPlayer);
        RpcShowCard(cardIndex);
    }

    [Server]
    private void HandleCardSelection(Card card, MgMemoryPlayer currentPlayer) {
        if (firstSelectedCard == null) {
            firstSelectedCard = card;
            return;
        }

        if (secondSelectedCard == null) {
            secondSelectedCard = card;
            StartCoroutine(CheckMatching(firstSelectedCard, secondSelectedCard, currentPlayer));

            firstSelectedCard = null;
            secondSelectedCard = null;
        }
    }

    [ClientRpc]
    public void RpcShowCard(int cardIndex) {
        var card = FindCardByIndex(cardIndex);
        if (card != null) {
            card.Show();
        }
    }

    [ClientRpc]
    public void RpcHideCard(int cardIndex) {
        Card card = FindCardByIndex(cardIndex);
        if (card != null) {
            card.Hide();
        }
    }

    [ClientRpc]
    public void RpcMarkCardsAsMatched(int firstCardIndex, int secondCardIndex) {
        var firstCard = FindCardByIndex(firstCardIndex);
        var secondCard = FindCardByIndex(secondCardIndex);

        if (firstCard != null && secondCard != null) {
            // Hier könnten Sie zusätzliche visuelle Effekte für gematchte Karten hinzufügen
            // z.B. eine andere Farbe oder einen Rahmen
        }
    }

    private Card FindCardByIndex(int cardIndex) {
        foreach (Transform child in gridTransform) {
            var card = child.GetComponent<Card>();
            if (card != null && card.CardIndex == cardIndex) {
                return card;
            }
        }
        return null;
    }

    [Server]
    private IEnumerator CheckMatching(Card firstCard, Card secondCard, MgMemoryPlayer currentPlayer) {
        yield return new WaitForSeconds(checkDelay);

        if (firstCard.GetIconSprite == secondCard.GetIconSprite) {
            Debug.Log("Match gefunden - Spieler bekommt Punkte");
        }
        else {
            // Kein Match - verstecke die Karten wieder
            RpcHideCard(firstCard.CardIndex);
            RpcHideCard(secondCard.CardIndex);
        }
    }

    public void ClearMemory() {
        foreach (Transform child in gridTransform) {
            Destroy(child.gameObject);
        }
    }
}