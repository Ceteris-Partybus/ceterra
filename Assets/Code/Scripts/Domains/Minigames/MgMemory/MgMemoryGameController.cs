using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MgMemoryGameController : NetworkedSingleton<MgMemoryGameController> {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private readonly float checkDelay = 0.5f;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;
    private List<Card> allCards = new List<Card>(); // Referenz auf alle Karten für Index-Zugriff

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
        Debug.Log($"Initializing cards on client with seed: {randomSeed}");

        Random.InitState(randomSeed);
        PrepareSprites();

        InitializeCards();
    }

    void InitializeCards() {
        allCards.Clear(); // Liste zurücksetzen
        
        for (var i = 0; i < spritePairs.Count; i++) {
            var card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.SetCardIndex(i); // Eindeutige ID setzen
            card.memoryController = this;
            
            allCards.Add(card); // Zur Liste hinzufügen für Index-Zugriff
        }
    }

    // Command: Client sendet Karten-Auswahl an Server
    [Command(requiresAuthority = false)]
    public void CmdSelectCard(int cardIndex) {
        if (cardIndex < 0 || cardIndex >= allCards.Count) {
            Debug.LogError($"Invalid card index: {cardIndex}");
            return;
        }

        Card selectedCard = allCards[cardIndex];
        
        if (selectedCard.IsSelected) {
            Debug.Log("Card already selected, ignoring");
            return;
        }

        Debug.Log($"Server: Card {cardIndex} selected");
        
        // RPC an alle Clients senden, um Karte aufzudecken
        RpcShowCard(cardIndex);
        
        // Server-seitige Spiellogik
        HandleCardSelection(selectedCard);
    }

    // RPC: Server informiert alle Clients über Karten-Aufdeckung
    [ClientRpc]
    private void RpcShowCard(int cardIndex) {
        if (cardIndex < 0 || cardIndex >= allCards.Count) {
            Debug.LogError($"Invalid card index in RPC: {cardIndex}");
            return;
        }

        Debug.Log($"Client: Showing card {cardIndex}");
        allCards[cardIndex].ShowLocal();
    }

    // RPC: Server informiert alle Clients über Karten-Verstecken
    [ClientRpc]
    private void RpcHideCard(int cardIndex) {
        if (cardIndex < 0 || cardIndex >= allCards.Count) {
            Debug.LogError($"Invalid card index in RPC: {cardIndex}");
            return;
        }

        Debug.Log($"Client: Hiding card {cardIndex}");
        allCards[cardIndex].HideLocal();
    }

    // Server-seitige Spiellogik (ersetzt SetSelectedCard)
    [Server]
    private void HandleCardSelection(Card card) {
        if (firstSelectedCard == null) {
            Debug.Log("First card selected on server");
            firstSelectedCard = card;
            return;
        }

        if (secondSelectedCard == null) {
            Debug.Log("Second card selected on server");
            secondSelectedCard = card;
            StartCoroutine(CheckMatching(firstSelectedCard, secondSelectedCard));

            firstSelectedCard = null;
            secondSelectedCard = null;
        }
    }

    [Server]
    private IEnumerator CheckMatching(Card firstCard, Card secondCard) {
        yield return new WaitForSeconds(checkDelay);

        if (firstCard.GetIconSprite == secondCard.GetIconSprite) {
            Debug.Log("Matched on server");
            // Karten bleiben aufgedeckt - keine weitere Aktion nötig
        }
        else {
            Debug.Log("Not Matched on server");
            // RPC an alle Clients senden, um Karten zu verstecken
            int firstCardIndex = allCards.IndexOf(firstCard);
            int secondCardIndex = allCards.IndexOf(secondCard);
            
            if (firstCardIndex >= 0) {
                RpcHideCard(firstCardIndex);
            }
            if (secondCardIndex >= 0) {
                RpcHideCard(secondCardIndex);
            }
        }
    }

    public void ClearMemory() {
        foreach (Transform child in gridTransform) {
            Destroy(child.gameObject);
        }
        
        // Listen zurücksetzen
        allCards.Clear();
        firstSelectedCard = null;
        secondSelectedCard = null;
    }
}