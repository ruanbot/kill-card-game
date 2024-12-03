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

    private int StartingHandSize = 5;
    private int currentCardCount = 0;

    private bool isHandInitialized = false;
    private bool highlightLock = false;
    public Card currentlyHoveredCard;

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
        GameObject cardVisual = InstantiateCardVisual(uniqueCard);

        if (isHandInitialized)
        {
            InitializeCardScripts(cardVisual);
        }
        else
        {
            // Increment the current card count
            currentCardCount++;

            // Check if the hand is fully instantiated
            if (currentCardCount == StartingHandSize)
            {
                InitializeAllCardScripts();
                isHandInitialized = true;
                Debug.Log("Hand fully initialized with 5 cards.");
            }
        }

        // Assign the card data to the CardHoverHighlight component
        CardHoverHighlight hoverHighlight = cardVisual.GetComponent<CardHoverHighlight>();
        if (hoverHighlight != null)
        {
            hoverHighlight.SetCard(uniqueCard); // Pass the card data to the hover script
        }

        // Assign the card data to the DisplayCard component (if applicable)
        DisplayCard displayCard = cardVisual.GetComponent<DisplayCard>();
        if (displayCard != null)
        {
            displayCard.card = uniqueCard; // Pass the card data to the display script
            displayCard.UpdateCardInfo(); // Update card visuals like name, artwork, etc.
        }
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

            ClearHighlights();

            DestroyCardVisual(card); // Remove the card visual
        }
        else
        {
            Debug.Log("Not enough energy to play this card!");
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

    public void OnCardHold(Card card)
    {
        // Find the ArcRenderer on the selected card
        var cardTransform = cardPrefab.transform;
        var arcRenderer = cardTransform.GetComponentInChildren<ArcRenderer>();

        if (arcRenderer != null)
        {
            arcRenderer.enabled = true; // Enable ArcRenderer
        }
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

    public bool IsHighlightLocked => highlightLock;

    public void LockHighlights(bool isLocked)
    {
        highlightLock = isLocked;
    }

    public void HighlightTargets(Card card)
    {
        if (highlightLock)
        {
            Debug.Log("Highlighting locked.");
            return; // Skip highlighting if locked
        }
        if (card.targetType == TargetType.All)
        {
            foreach (var entity in battleSystem.allBattlers)
            {
                entity.BattleVisuals?.SetHighlight(true);
            }
        }
        else if (card.targetType == TargetType.AllEnemy)
        {
            foreach (var enemy in battleSystem.enemyBattlers)
            {
                enemy.BattleVisuals?.SetHighlight(true);
            }
        }
        else if (card.targetType == TargetType.AllFriendly)
        {
            foreach (var friendly in battleSystem.playerBattlers)
            {
                friendly.BattleVisuals?.SetHighlight(true);
            }
        }
        else if (card.targetType == TargetType.Enemy && battleSystem.enemyBattlers.Count == 1)
        {
            battleSystem.enemyBattlers[0].BattleVisuals?.SetHighlight(true);
            Debug.Log($"hovering over cards: {card.cardName}");
        }
        else if (card.targetType == TargetType.Friendly && battleSystem.playerBattlers.Count == 1)
        {
            battleSystem.playerBattlers[0].BattleVisuals?.SetHighlight(true);
        }
    }

    public void ClearHighlights()
    {
        foreach (var entity in battleSystem.allBattlers)
        {
            entity.BattleVisuals?.SetHighlight(false);
        }
    }

    public void SetCurrentlyHoveredCard(Card card)
    {
        currentlyHoveredCard = card;
    }

    public void ClearCurrentlyHoveredCard()
    {
        currentlyHoveredCard = null;
    }


    private GameObject InstantiateCardVisual(Card card)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned in the CardManager.");
            return null;
        }

        Transform handTransform = GameObject.Find("Hand").transform; // Replace "Hand" with the correct name of your hand UI
        if (handTransform == null)
        {
            Debug.LogError("Hand UI panel not found in the scene.");
            return null;
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

        return cardVisual;
    }


    public void OnCardRelease(Card card)
    {
        ClearHighlights();

        var cardTransform = cardPrefab.transform;
        var arcRenderer = cardTransform.GetComponentInChildren<ArcRenderer>();

        if (arcRenderer != null)
        {
            arcRenderer.enabled = false; // Disable ArcRenderer
        }
    }

    public void OnCardClicked(Card card)
    {
        if (selectedCard == card)
        {
            Debug.Log("Card already selected, deselecting.");
            OnCardRelease(card);
            DeselectCard();
        }
        else
        {
            if (selectedCard != null) OnCardRelease(selectedCard);
            Debug.Log($"Card selected: {card.cardName}");
            SelectCard(card);
            OnCardHold(card); // Enable ArcRenderer on hold
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

    private void InitializeCardScripts(GameObject cardVisual)
    {
        // Initialize CardHover script
        var cardHover = cardVisual.GetComponentInChildren<CardHover>();
        if (cardHover != null)
        {
            cardHover.InitializeHover();
            Debug.Log("Initialized CardHover script for card.");
        }

        // Initialize CardOverlapHandler script
        var cardOverlap = cardVisual.GetComponent<CardOverlapHandler>();
        if (cardOverlap != null)
        {
            cardOverlap.InitializeOverlap();
            Debug.Log("Initialized CardOverlapHandler script for card.");
        }
    }

    private void InitializeAllCardScripts()
    {
        Transform handTransform = GameObject.Find("Hand").transform;
        if (handTransform == null)
        {
            Debug.LogError("Hand UI panel not found in the scene.");
            return;
        }

        foreach (Transform cardTransform in handTransform)
        {
            InitializeCardScripts(cardTransform.gameObject);
        }
    }


}
