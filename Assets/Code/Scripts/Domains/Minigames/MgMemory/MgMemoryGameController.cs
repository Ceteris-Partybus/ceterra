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
                MgMemoryController.Instance.UpdatePlayerScore(currentPlayer.Score + pointsForCorrectMatch);
                currentPlayer.AddScore(pointsForCorrectMatch);
                CmdMarkCardsAsMatched(currentPlayer.FirstSelectedCardIndex, currentPlayer.SecondSelectedCardIndex);

                var matchedCard = FindCardByIndex(currentPlayer.FirstSelectedCardIndex);
                matchedCard.SetFactData("Partybus", "This is a fact about the Partybus.", "Minigame/Memory/og_2");

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
        MgMemoryController.Instance.ShowFactPopup(factData);
    }

    [Command(requiresAuthority = false)]
    private void CmdHandleMismatch() {
        MgMemoryContext.Instance.HandleMismatch();
    }

    [Server]
    private IEnumerator HideCardsOnAllClientsWithDelay(int firstCardIndex, int secondCardIndex) {
        yield return new WaitForSeconds(checkDelay);

        RpcHideCard(firstCardIndex);
        RpcHideCard(secondCardIndex);
    }

    [ClientRpc]
    public void RpcShowCard(int cardIndex) {
        FindCardByIndex(cardIndex).Show();
    }

    [ClientRpc]
    public void RpcHideCard(int cardIndex) {
        FindCardByIndex(cardIndex).Hide();

    }

    [ClientRpc]
    public void RpcMarkCardsAsMatched(int firstCardIndex, int secondCardIndex) {
        FindCardByIndex(firstCardIndex).SetButtonInteractable(false);
        FindCardByIndex(secondCardIndex).SetButtonInteractable(false);

        // Hier könnten Sie zusätzliche visuelle Effekte für gematchte Karten hinzufügen
        // z.B. eine andere Farbe oder einen Rahmen
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