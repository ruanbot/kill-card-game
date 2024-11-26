using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] private List<Card> hand = new List<Card>();
    public IReadOnlyList<Card> Hand => hand;

    [SerializeField] private GameObject cardPrefab;


    private Card selectedCard; // The card currently selected by the player
    private EnergyManager energyManager;
    private BattleSystem battleSystem;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keep the manager across scenes if needed.
    }

    private void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }

    public void AddToHand(Card card)
    {
        Card uniqueCard = Instantiate(card);
        hand.Add(uniqueCard);
        InstantiateCardVisual(uniqueCard);
    }


    public void AddCardToHand(Card card)
    {
        hand.Add(card);
    }

    public void RemoveCardFromHand(Card card)
    {
        hand.Remove(card);
    }


    public void PlayCard(Card card, BattleEntities caster, BattleEntities target)
    {

        bool found = false;
        foreach (var c in hand)
        {
            if (c == card) // Use reference equality explicitly
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            return;
        }

        if (energyManager.HasEnoughEnergy(card.manaCost))
        {
            Debug.Log($"Playing card {card.cardName}. Caster: {caster.Name}, Target: {target.Name}, EntityType: {target.EntityType}");

            hand.Remove(card);

            // Execute card's effect
            card.Use(caster, target);

            // Update energy manager
            energyManager.SpendEnergy(card.manaCost);

            // Notify PlayerDeck to move card to discard pile
            FindFirstObjectByType<PlayerDeck>().DiscardCard(card);

            DestroyCardVisual(card); // Remove the card visual
            Debug.Log($"Played card {card.cardName}. Hand count: {hand.Count}");
        }
        else
        {
            Debug.Log("Not enough energy to play this card!");
        }
    }



    public void OnCardClicked(Card card)
    {
        if (selectedCard == card)
        {
            Debug.Log("Card already selected, deselecting.");
            DeselectCard();
        }
        else
        {
            if (selectedCard != null) DeselectCard();
            Debug.Log($"Card selected: {card.cardName}");
            SelectCard(card);
        }
    }


    private void SelectCard(Card card)
    {
        selectedCard = card;

        var caster = battleSystem.GetCurrentPlayer(); // Get the active player/caster
        Debug.Log($"Selected card: {card.cardName}, Caster: {caster.Name}");

        if (card.targetType == TargetType.Self)
        {
            PlayCard(card, caster, caster); // Play on self
        }
        else if (card.targetType == TargetType.Enemy)
        {
            if (battleSystem.enemyBattlers.Count == 1)
            {
                var target = battleSystem.enemyBattlers[0];
                Debug.Log($"Auto-selecting enemy target: {target.Name}, EntityType: {target.EntityType}");
                PlayCard(card, caster, target);
            }
            else
            {
                Debug.Log("Select an enemy target.");
                EnableTargeting(battleSystem.enemyBattlers);
            }
        }
        else if (card.targetType == TargetType.Friendly)
        {
            if (battleSystem.playerBattlers.Count == 1)
            {
                var target = battleSystem.playerBattlers[0];
                Debug.Log($"Auto-selecting friendly target: {target.Name}, EntityType: {target.EntityType}");
                PlayCard(card, caster, target);
            }
            else
            {
                Debug.Log("Select a friendly target.");
                EnableTargeting(battleSystem.playerBattlers);
            }
        }

        DeselectCard(); // Ensure card is deselected after targeting
    }

    private void DeselectCard()
    {
        selectedCard = null;
        Debug.Log("Card deselected.");
    }

    private void EnableTargeting(List<BattleEntities> targets)
    {
        foreach (var target in targets)
        {
            target.BattleVisuals.SubscribeToTargetSelected(OnTargetSelected);
        }
    }


    private void OnTargetSelected(BattleVisuals targetVisual)
    {
        var target = battleSystem.allBattlers.Find(entity => entity.BattleVisuals == targetVisual);

        if (target != null && selectedCard != null)
        {
            var caster = battleSystem.GetCurrentPlayer();
            Debug.Log($"Playing card {selectedCard.cardName}. Caster: {caster.Name}, Target: {target.Name}");
            PlayCard(selectedCard, caster, target);
            DeselectCard();
        }
    }

    private void InstantiateCardVisual(Card card)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned in the CardManager.");
            return;
        }

        Transform handTransform = GameObject.Find("Hand").transform; // Replace "Hand" with the correct name of your hand UI
        if (handTransform == null)
        {
            Debug.LogError("Hand UI panel not found in the scene.");
            return;
        }

        GameObject cardVisual = Instantiate(cardPrefab, handTransform, false);
        DisplayCard displayCard = cardVisual.GetComponent<DisplayCard>();
        if (displayCard != null)
        {
            displayCard.card = card; // Assign the exact card reference
            displayCard.UpdateCardInfo(); // Update visuals like name, artwork, etc.
        }
        else
        {
            Debug.LogWarning("Card prefab is missing a DisplayCard component.");
        }
    }


    private void DestroyCardVisual(Card card)
    {
        // Locate the visual representation of the card in the hand
        Transform handTransform = GameObject.Find("Hand").transform; // Replace with your hand UI reference

        foreach (Transform child in handTransform)
        {
            DisplayCard displayCard = child.GetComponent<DisplayCard>();
            if (displayCard != null && displayCard.card == card)
            {
                Destroy(child.gameObject); // Destroy the visual GameObject
                Debug.Log($"Card visual destroyed: {card.cardName}");
                break;
            }
        }
    }
}
