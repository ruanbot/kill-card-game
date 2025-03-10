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
    private DisplayCard selectedVisual; //Disket Visual currently selected by the player 
    private EnergyManager energyManager;
    private BattleSystem battleSystem;

    private int StartingHandSize = 5;
    private int currentCardCount = 0;

    private bool isHandInitialized = false;
    private bool highlightLock = false;
    public Card currentlyHoveredCard;


    //Disket
    private List<DisplayCard> displayCardList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keep the manager across scenes if needed.
        //Disket
        displayCardList = new List<DisplayCard>();
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
        DisplayCard cardVisual = InstantiateCardVisual(uniqueCard);

        if (isHandInitialized)
        {
            InitializeCardScripts(cardVisual.gameObject);
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
        if (!hand.Contains(card)) return;

        if (energyManager.HasEnoughEnergy(card.manaCost))
        {
            // Remove the card from hand to maintain correct hand state
            hand.Remove(card);

            // Execute card effects
            card.Use(caster, target);

            // Trigger 'OnCardUse' effects
            caster.TriggerEffects(EffectTriggerType.OnCardUse);
            target.TriggerEffects(EffectTriggerType.OnCardUse);

            // Update Energy and discard card
            energyManager.SpendEnergy(card.manaCost);
            FindFirstObjectByType<PlayerDeck>().DiscardCard(card);

            // Clear Highlights & Destroy visual
            ClearHighlights();
            DestroyCardVisual(card);
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
        else if (card.targetType == TargetType.AllEnemy)
        {
            // Immediately execute the card effect on all enemies
            Debug.Log($"Playing {card.cardName} on all enemies.");
            foreach (var enemy in battleSystem.enemyBattlers)
            {
                PlayCard(card, caster, enemy);
            }
        }
        else if (card.targetType == TargetType.AllFriendly)
        {
            // Immediately execute the card effect on all friendly entities
            Debug.Log($"Playing {card.cardName} on all friendly entities.");
            foreach (var friendly in battleSystem.playerBattlers)
            {
                PlayCard(card, caster, friendly);
            }
        }
        else if (card.targetType == TargetType.All)
        {
            // Immediately execute the card effect on all entities
            Debug.Log($"Playing {card.cardName} on all entities.");
            foreach (var entity in battleSystem.allBattlers)
            {
                PlayCard(card, caster, entity);
            }
        }

    }

    public void OnCardHold(DisplayCard displayCard)
    {
        // Find the ArcRenderer on the selected card
        var arcRenderer = displayCard.GetComponentInChildren<CardHover>().GetComponentInChildren<ArcDotControllerUI>();
        selectedVisual = displayCard;

        if (selectedVisual != null)
        {
            selectedVisual.GetComponentInChildren<CardHover>().SetHover(true);
        }

        if (arcRenderer != null)
        {
            arcRenderer.enabled = true; // Enable ArcRenderer
        }
    }


    public void DeselectCard()
    {
        selectedCard = null;
        if (selectedVisual != null)
        {
            selectedVisual.GetComponentInChildren<CardHover>().SetHover(false);
        }

    }

    private void EnableTargeting(List<BattleEntities> targets)
    {
        foreach (var target in targets)
        {
            target.BattleVisuals.SubscribeToTargetSelected(OnTargetSelected);
            //Disket
            target.BattleVisuals.SubscribeToTargetHovering(OnTargetHover);
            target.BattleVisuals.SubscribeToTargetHoveringEnded(OnTargetHoverEnd);
        }
    }


    private void OnTargetSelected(BattleVisuals targetVisual)
    {
        BattleEntities target = battleSystem.allBattlers.Find(entity => entity.BattleVisuals == targetVisual);



        if (target == null)
        {
            Debug.Log("Selected target is null");
        }

        if (selectedCard == null)
        {
            Debug.Log("Selected card is null");
        }

        if (target != null && selectedCard != null)
        {
            Debug.Log("Successfully Played The Card");
            var caster = battleSystem.GetCurrentPlayer();
            Debug.Log($"Playing card {selectedCard.cardName}. Caster: {caster.Name}, Target: {target.Name}");
            PlayCard(selectedCard, caster, target);
            DeselectCard();
        }
    }

    //Disket
    private void OnTargetHover(BattleVisuals targetVisual)
    {
        //Disket
        if (selectedVisual != null)
        {

            selectedVisual.gameObject.GetComponentInChildren<CardHover>().GetComponentInChildren<ArcDotControllerUI>().SetTarget(targetVisual.transform.position);
        }
    }

    private void OnTargetHoverEnd(BattleVisuals targetVisual)
    {
        if (selectedVisual == null)
            return;

        selectedVisual.gameObject.GetComponentInChildren<CardHover>().GetComponentInChildren<ArcDotControllerUI>().DeselectTarget();
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

        // Debug.Log($"HighlightTargets called for {card.cardName} with TargetType: {card.targetType}");

        switch (card.targetType)
        {
            case TargetType.Self:
                var caster = battleSystem.GetCurrentPlayer();
                caster.BattleVisuals?.SetHighlight(true);
                // Debug.Log($"Auto-highlighting self: {caster.Name}");
                break;

            case TargetType.Friendly:
                if (battleSystem.playerBattlers.Count == 1)
                {
                    var friendlyTarget = battleSystem.playerBattlers[0];
                    friendlyTarget.BattleVisuals?.SetHighlight(true);
                    // Debug.Log($"Auto-highlighting friendly target: {friendlyTarget.Name}");
                }
                break;

            case TargetType.Enemy:
                if (battleSystem.enemyBattlers.Count == 1)
                {
                    var enemyTarget = battleSystem.enemyBattlers[0];
                    enemyTarget.BattleVisuals?.SetHighlight(true);
                    // Debug.Log($"Auto-highlighting enemy target: {enemyTarget.Name}");
                }
                break;

            case TargetType.All:
                foreach (var entity in battleSystem.allBattlers)
                {
                    entity.BattleVisuals?.SetHighlight(true);
                   // Debug.Log($"Highlighting all entities: {entity.Name}");
                }
                break;

            case TargetType.AllFriendly:
                foreach (var friendly in battleSystem.playerBattlers)
                {
                    friendly.BattleVisuals?.SetHighlight(true);
                    // Debug.Log($"Highlighting all friendly entities: {friendly.Name}");
                }
                break;

            case TargetType.AllEnemy:
                foreach (var enemy in battleSystem.enemyBattlers)
                {
                    enemy.BattleVisuals?.SetHighlight(true);
                    // Debug.Log($"Highlighting all enemy entities: {enemy.Name}");
                }
                break;

            default:
                // Debug.LogWarning($"Unhandled TargetType: {card.targetType}");
                break;
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


    //Disket (Changed return type to DisplayCard)
    private DisplayCard InstantiateCardVisual(Card card)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned in the CardManager.");
            return null;
        }

        Transform handTransform = GameObject.Find("Hand").transform;
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

        //Disket
        return cardVisual.GetComponent<DisplayCard>();
    }


    public void OnCardRelease(Card card)
    {
        ClearHighlights();
    }

    public void OnCardRelease(DisplayCard card)
    {
        OnCardRelease(card.card);

        // Find the ArcRenderer on the selected card
        var arcRenderer = card.GetComponentInChildren<CardHover>().GetComponentInChildren<ArcDotControllerUI>();

        if (arcRenderer != null)
        {
            arcRenderer.enabled = false; // Disable ArcRenderer
        }
    }

    public void OnCardClicked(Card card)
    {
        if (selectedCard == card)
        {
            // Debug.Log("Card already selected, deselecting.");
            OnCardRelease(card);
            DeselectCard();
        }
        else
        {
            if (selectedCard != null) OnCardRelease(selectedCard);
            // Debug.Log($"Card selected: {card.cardName}");
            SelectCard(card);
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
                break;
            }
        }
    }

    public bool IsHandInitialized()
    {
        return isHandInitialized;
    }

    private void InitializeCardScripts(GameObject cardVisual)
    {
        // Initialize CardHover script
        var cardHover = cardVisual.GetComponentInChildren<CardHover>();
        if (cardHover != null)
        {
            cardHover.InitializeHover();
            // Debug.Log("Initialized CardHover script for card.");
        }

        // Initialize CardOverlapHandler script
        var cardOverlap = cardVisual.GetComponent<CardOverlapHandler>();
        if (cardOverlap != null)
        {
            cardOverlap.InitializeOverlap();
            // Debug.Log("Initialized CardOverlapHandler script for card.");
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

        isHandInitialized = true;  //  Mark hand as initialized
        FindFirstObjectByType<BattleSystem>()?.StartEnemyAttacks();  //  Notify BattleSystem
    }


}
