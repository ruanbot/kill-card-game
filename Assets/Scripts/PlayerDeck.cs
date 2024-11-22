using System.Collections;
using System.Collections.Generic; // Keep this only if you need List<> or other generic collections
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    [Header("Deck Settings")]
    public List<Card> deck = new List<Card>();
    public List<Card> discardPile = new List<Card>();

    private CardManager cardManager;

    void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>();
        ShuffleDeck();
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 5; i++) // Draw initial 5 cards
        {
            yield return new WaitForSeconds(0.5f);
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (deck.Count == 0) ShuffleDiscardIntoDeck();
        if (deck.Count == 0) return; // No cards to draw

        Card drawnCard = deck[0];
        deck.RemoveAt(0);

        // Ensure drawnCard is not null and only added once
        if (drawnCard == null)
        {
            Debug.LogWarning("Drawn card is null.");
            return;
        }

        // Notify CardManager to add the card to the hand
        if (cardManager != null)
        {
            cardManager.AddToHand(drawnCard);
        }
        else
        {
            Debug.LogError("CardManager reference is null.");
        }
    }


    public void DiscardCard(Card card)
    {
        discardPile.Add(card);
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    private void ShuffleDiscardIntoDeck()
    {
        if (discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
    }
}
