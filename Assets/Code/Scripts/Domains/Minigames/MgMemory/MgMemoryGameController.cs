using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MgMemoryGameController : NetworkedSingleton<MgMemoryGameController> {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private readonly float checkDelay = 0.7f;
    [SerializeField] private int pointsForCorrectMatch = 1;

    private List<Sprite> spritePairs;
    private List<MemoryFactData> currentMemoryFacts;

    protected override void Start() {
        base.Start();
        spritePairs = new();
    }

    public void PrepareSprites() {
        spritePairs = new();

        foreach (var fact in currentMemoryFacts) {
            var sprite = Resources.Load<Sprite>(fact.imagePath);
            spritePairs.Add(sprite);
            spritePairs.Add(sprite);
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

    public void InitializeCardsOnClients(int randomSeed, List<MemoryFactData> memoryFacts) {
        currentMemoryFacts = memoryFacts;
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

            var factData = GetFactDataForSprite(spritePairs[i]);
            card.SetFactData(factData.title, factData.description, factData.imagePath);
        }
    }

    private MemoryFactData GetFactDataForSprite(Sprite sprite) {
        foreach (var fact in currentMemoryFacts) {
            var factSprite = Resources.Load<Sprite>(fact.imagePath);
            if (factSprite == sprite) {
                return fact;
            }
        }
        return null;
    }

    public void SelectCard(int cardIndex, MgMemoryPlayer currentPlayer) {
        if (MgMemoryContext.Instance.GetCurrentPlayer() != currentPlayer) {
            Debug.Log("It's not this player's turn!");
            return;
        }

        CmdShowCardOnAllClients(cardIndex);

        if (!currentPlayer.HasFirstSelection) {
            currentPlayer.SetFirstSelectedCard(cardIndex);
        }
        else {
            currentPlayer.SetSecondSelectedCard(cardIndex);

            var match =
                FindCardByIndex(currentPlayer.FirstSelectedCardIndex).GetIconSprite ==
                FindCardByIndex(currentPlayer.SecondSelectedCardIndex).GetIconSprite;

            if (match) {
                currentPlayer.AddScore(pointsForCorrectMatch);
                currentPlayer.CmdAddScore(pointsForCorrectMatch); // Für Auswertung der Punkte -> Scoreboard

                MgMemoryController.Instance.UpdatePlayerScore(currentPlayer.Score);
                CmdMarkCardsAsMatched(currentPlayer.FirstSelectedCardIndex, currentPlayer.SecondSelectedCardIndex);

                var matchedCard = FindCardByIndex(currentPlayer.FirstSelectedCardIndex);
                CmdHandleMatch(matchedCard.FactData);
            }
            else {
                CmdHideCardsOnAllClients(currentPlayer.FirstSelectedCardIndex, currentPlayer.SecondSelectedCardIndex);
                CmdHandleMismatch();
            }

            currentPlayer.ClearCardSelections();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdShowCardOnAllClients(int cardIndex) {
        RpcShowCard(cardIndex);
    }

    [Command(requiresAuthority = false)]
    private void CmdHideCardsOnAllClients(int firstCardIndex, int secondCardIndex) {
        StartCoroutine(HideCardsOnAllClientsWithDelay(firstCardIndex, secondCardIndex));
    }

    [Command(requiresAuthority = false)]
    private void CmdMarkCardsAsMatched(int firstCardIndex, int secondCardIndex) {
        RpcMarkCardsAsMatched(firstCardIndex, secondCardIndex);
    }

    [Command(requiresAuthority = false)]
    private void CmdHandleMatch(MemoryFactData factData) {
        MgMemoryContext.Instance.ShowFactPopupWithDuration(factData);
    }

    [Command(requiresAuthority = false)]
    private void CmdHandleMismatch() {
        MgMemoryContext.Instance.HandleMismatch();
    }

    [Server]
    private IEnumerator HideCardsOnAllClientsWithDelay(int firstCardIndex, int secondCardIndex) {
        yield return new WaitForSeconds(checkDelay);

        RpcHideCards(firstCardIndex, secondCardIndex);
    }

    [ClientRpc]
    public void RpcShowCard(int cardIndex) {
        FindCardByIndex(cardIndex)?.Show();
    }

    [ClientRpc]
    public void RpcHideCards(int firstCardIndex, int secondCardIndex) {
        FindCardByIndex(firstCardIndex).ShakeAndFlip();
        FindCardByIndex(secondCardIndex).ShakeAndFlip(playSound: false);
    }

    [ClientRpc]
    public void RpcMarkCardsAsMatched(int firstCardIndex, int secondCardIndex) {
        FindCardByIndex(firstCardIndex)?.SetButtonInteractable(false);
        FindCardByIndex(secondCardIndex)?.SetButtonInteractable(false);
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

    public void ClearMemory() {
        foreach (Transform child in gridTransform) {
            Destroy(child.gameObject);
        }
    }
}