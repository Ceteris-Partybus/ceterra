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
        for (var i = 0; i < spritePairs.Count; i++) {
            var card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.memoryController = this;
        }
    }

    public void SetSelectedCard(Card card) {
        if (!card.IsSelected) {
            Debug.Log("Card selected");
            card.Show();

            if (firstSelectedCard == null) {
                Debug.Log("First card selected");
                firstSelectedCard = card;
                return;
            }

            if (secondSelectedCard == null) {
                Debug.Log("Second card selected");
                secondSelectedCard = card;
                StartCoroutine(CheckMatching(firstSelectedCard, secondSelectedCard));

                firstSelectedCard = null;
                secondSelectedCard = null;
            }
        }
    }

    private IEnumerator CheckMatching(Card firstCard, Card secondCard) {
        yield return new WaitForSeconds(checkDelay);

        if (firstCard.GetIconSprite == secondCard.GetIconSprite) {
            Debug.Log("Matched");
        }
        else {
            Debug.Log("Not Matched");
            firstCard.Hide();
            secondCard.Hide();
        }
    }

    public void ClearMemory() {
        foreach (Transform child in gridTransform) {
            Destroy(child.gameObject);
        }
    }
}