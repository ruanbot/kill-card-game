using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDeck : MonoBehaviour
{
    [Header("Deck Settings")]
    public List<Card> deck = new List<Card>();
    public List<Card> discardPile = new List<Card>();

    [Header("UI")]
    public TMP_Text deckCountText;
    public Button drawButton;

    [Header("Draw Cost")]
    public int drawManaCost = 1;

    private CardManager cardManager;
    private EnergyManager energyManager;

    void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>();
        energyManager = FindFirstObjectByType<EnergyManager>();

        // Disable draw button during initial deal
        if (drawButton != null)
            drawButton.interactable = false;

        ShuffleDeck();
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 5; i++) // Draw initial 5 cards
        {
            yield return new WaitForSeconds(0.5f);
            DrawCard(true); // free draw during setup
        }

        // Re-enable draw button after hand is fully dealt
        if (drawButton != null)
            drawButton.interactable = true;
    }

    /// <summary>
    /// Draw a card. Called by the Draw button (costs mana) or internally during setup (free).
    /// </summary>
    public void DrawCard()
    {
        DrawCard(false);
    }

    private void DrawCard(bool free)
    {
        if (deck.Count == 0) ShuffleDiscardIntoDeck();
        if (deck.Count == 0) return; // No cards to draw

        // Check mana cost (skip during free draws like initial deal)
        if (!free)
        {
            if (energyManager == null)
            {
                energyManager = FindFirstObjectByType<EnergyManager>();
            }

            if (energyManager != null && !energyManager.HasEnoughEnergy(drawManaCost))
            {
                Debug.Log($"Not enough energy to draw. Need {drawManaCost}, have {energyManager.GetCurrentEnergy()}");
                return;
            }

            // Spend the mana
            if (energyManager != null)
            {
                energyManager.SpendEnergy(drawManaCost);
            }
        }

        Card drawnCard = deck[0];
        deck.RemoveAt(0);
        UpdateDeckCountUI();

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
            UpdateDeckCountUI();
        }
    }

    private void UpdateDeckCountUI()
    {
        if (deckCountText != null)
            deckCountText.text = deck.Count.ToString();
    }
}
