using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    [Header("Deck Settings")]
    public List<Card> deck = new List<Card>();
    // public static List<Card> staticDeck = new List<Card>();
    public List<Card> discardPile = new List<Card>();
    public List<Card> hand = new List<Card>();

    public GameObject CardToHand; // Prefab for CardToHand (card visual in hand)
    public GameObject Hand; // Reference to the Hand UI panel

    public GameObject cardInDeck1;
    public GameObject cardInDeck2;
    public GameObject cardInDeck3;
    public GameObject cardInDeck4;

    private EnergyManager energyManager;
    private DiscardPileManager discardPileManager;

    void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        discardPileManager = FindFirstObjectByType<DiscardPileManager>();
        ShuffleDeck();
        StartCoroutine(StartGame());
    }

    private void Update()
    {
        UpdateDeckDisplay();
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i <= 4; i++)
        {
            yield return new WaitForSeconds(0.5f);
            DrawCard();

        }
    }

    public void ManualDrawCard()
    {
        if (energyManager.HasEnoughEnergy(2))
        {
            DrawCard();
            energyManager.SpendEnergy(2);
        }
    }

    public void DrawCard()
    {
        // Check if the deck is empty and shuffle discard pile if needed
        if (deck.Count == 0)
        {
            ShuffleDiscardIntoDeck();
        }

        if (deck.Count > 0)
        {
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);

            // Instantiate CardToHand prefab for visual representation
            GameObject newCardVisual = Instantiate(CardToHand, Hand.transform, false);
            DisplayCard displayCard = newCardVisual.GetComponent<DisplayCard>();

            if (displayCard != null)
            {
                displayCard.card = drawnCard; // Set card data on DisplayCard component
                displayCard.UpdateCardInfo(); // Update visuals based on card data
            }
        }
    }


    public void UpdateDeckDisplay()
    {
        if (deck.Count < 7) cardInDeck1.SetActive(false);
        if (deck.Count < 5) cardInDeck2.SetActive(false);
        if (deck.Count < 2) cardInDeck3.SetActive(false);
        if (deck.Count < 1) cardInDeck4.SetActive(false);
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void ShuffleDiscardIntoDeck()
    {
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            // Move all cards from discard pile to deck
            deck.AddRange(discardPile);
            discardPile.Clear();
            discardPileManager.ClearDiscardPileVisuals();

            // Shuffle the deck
            ShuffleDeck();
        }
    }

}
