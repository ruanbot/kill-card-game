using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


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
    private bool cardsPlayable = true;
    public Card currentlyHoveredCard;
    private CombatActionQueue actionQueue;


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
        actionQueue = battleSystem.ActionQueue;
    }

    public void SetCardsPlayable(bool playable)
    {
        cardsPlayable = playable;
    }

    [Header("Draw Animation")]
    [SerializeField] private float drawAnimDuration = 0.3f;

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

        UpdateHandSpacing();

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

        // Animate card from deck to its hand position
        StartCoroutine(AnimateCardFromDeck(cardVisual.gameObject));
    }

    private IEnumerator AnimateCardFromDeck(GameObject cardObj)
    {
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        Transform handTransform = cardObj.transform.parent;

        // Disable hover/spread on this card during animation
        var overlapHandler = cardObj.GetComponent<CardOverlapHandler>();
        if (overlapHandler != null)
        {
            overlapHandler.isAnimating = true;
            overlapHandler.animTargetPos = cardRect.anchoredPosition;
        }

        Vector3 targetScale = cardRect.localScale; // 0.55

        // Find DeckPanel and convert its position to Hand's local space
        GameObject deckPanel = GameObject.Find("DeckPanel");
        if (deckPanel != null)
        {
            Vector3 deckWorldPos = deckPanel.transform.position;
            Vector3 deckLocalPos = handTransform.InverseTransformPoint(deckWorldPos);
            cardRect.anchoredPosition = new Vector2(deckLocalPos.x, deckLocalPos.y);
        }

        // Start small
        Vector3 startScale = targetScale * 0.5f;
        cardRect.localScale = startScale;

        // Use a chase speed instead of fixed lerp from start→end.
        // This way, if animTargetPos changes mid-flight (because more cards were drawn
        // and UpdateHandSpacing recalculated), the card smoothly redirects without
        // snapping or tilting through a wrong intermediate path.
        float chaseSpeed = 1f / drawAnimDuration * 3f; // reaches target well within duration
        float elapsed = 0f;
        while (elapsed < drawAnimDuration)
        {
            elapsed += Time.deltaTime;
            float dt = Time.deltaTime * chaseSpeed;

            Vector2 targetPos = overlapHandler != null ? overlapHandler.animTargetPos : cardRect.anchoredPosition;
            cardRect.anchoredPosition = Vector2.Lerp(cardRect.anchoredPosition, targetPos, dt);
            cardRect.localScale = Vector3.Lerp(cardRect.localScale, targetScale, dt);

            yield return null;
        }

        // Snap to exact final values
        Vector2 finalPos = overlapHandler != null ? overlapHandler.animTargetPos : cardRect.anchoredPosition;
        cardRect.anchoredPosition = finalPos;
        cardRect.localScale = targetScale;

        // Re-enable hover/spread
        if (overlapHandler != null)
            overlapHandler.isAnimating = false;
    }

    public void RemoveCardFromHand(Card card)
    {
        hand.Remove(card);
    }


    public void PlayCard(Card card, BattleEntities caster, BattleEntities target)
    {
        if (!hand.Contains(card)) return;
        if (!cardsPlayable) return;

        if (energyManager.HasEnoughEnergy(card.manaCost))
        {
            // Immediate: remove from hand, spend energy, discard, destroy visual
            hand.Remove(card);
            energyManager.SpendEnergy(card.manaCost);
            FindFirstObjectByType<PlayerDeck>().DiscardCard(card);
            ClearHighlights();
            DestroyCardVisual(card);

            // Check if this is an attack card (has a damage-dealing Use)
            bool isAttackCard = card is SlashData || card is CleaveData;

            // Queue the execution
            actionQueue.Enqueue(new CombatAction
            {
                description = $"Player plays {card.cardName}",
                enqueuedTime = Time.time,
                isPlayerAction = true,
                execute = (done) =>
                {
                    if (isAttackCard)
                    {
                        // Play attack animation, then apply card effect, then react
                        caster.BattleVisuals?.PlayAttackAnimation(() =>
                        {
                            card.Use(caster, target);
                            caster.TriggerEffects(EffectTriggerType.OnCardUse);
                            target.TriggerEffects(EffectTriggerType.OnCardUse);

                            bool tookDamage = !target.IsAlive || target.CurrentHealth < target.MaxHealth;
                            if (!target.IsAlive)
                            {
                                target.BattleVisuals?.PlayDeathAnimation(() =>
                                {
                                    battleSystem.RemoveDeadEntity(target);
                                    if (target.BattleVisuals != null)
                                        UnityEngine.Object.Destroy(target.BattleVisuals.gameObject, 1f);
                                    done();
                                });
                            }
                            else if (tookDamage)
                            {
                                target.BattleVisuals?.PlayHitAnimation(() => done());
                            }
                            else
                            {
                                done();
                            }
                        });
                    }
                    else
                    {
                        // Non-attack card (buff, heal, etc.) — just execute immediately
                        card.Use(caster, target);
                        caster.TriggerEffects(EffectTriggerType.OnCardUse);
                        target.TriggerEffects(EffectTriggerType.OnCardUse);
                        done();
                    }
                }
            });
        }
        else
        {
            Debug.Log("Not enough energy to play this card!");
        }
    }


    private void SelectCard(Card card)
    {
        if (!cardsPlayable) return;
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

        // Scale cards down so they fit comfortably in the hand
        cardVisual.transform.localScale = Vector3.one * 0.55f;

        // Set anchor and pivot to center so manual positioning works correctly
        var cardRect = cardVisual.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
        }

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

        // Set up sorting immediately so cards overlap correctly during dealing
        // (left cards render on top of right cards). InitializeOverlap() will
        // reuse this Canvas later since it checks for null before adding.
        var canvas = cardVisual.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = cardVisual.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
        }
        // Left cards get higher order so they render on top
        int childCount = handTransform.childCount;
        int index = cardVisual.transform.GetSiblingIndex();
        canvas.sortingOrder = childCount - index;

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
                // Clear spread state NOW (before Destroy, which is deferred to end of frame).
                // Without this, if the mouse is still over the hand, a neighbor card could
                // receive HandlePointerEnter and snapshot the OLD layout positions before
                // UpdateHandSpacing recalculates for the reduced hand.
                CardOverlapHandler.ResetStaticState();

                Destroy(child.gameObject);

                // Refresh sorting orders next frame after the object is actually destroyed
                StartCoroutine(RefreshSortingOrdersNextFrame(handTransform));
                break;
            }
        }
    }

    private System.Collections.IEnumerator RefreshSortingOrdersNextFrame(Transform handTransform)
    {
        yield return null; // wait for Destroy to take effect
        CardOverlapHandler.RefreshBaseSortingOrders(handTransform);
        UpdateHandSpacing();
    }

    public bool IsHandInitialized()
    {
        return isHandInitialized;
    }

    /// <summary>
    /// Manually positions cards in the Hand, centered, with dynamic overlap.
    /// Disables the HorizontalLayoutGroup because it ignores localScale when
    /// positioning children (uses rect.width=490 instead of visual width=269.5).
    /// </summary>
    private void UpdateHandSpacing()
    {
        Transform handTransform = GameObject.Find("Hand")?.transform;
        if (handTransform == null) return;

        // Disable the layout group — we position cards manually
        var layoutGroup = handTransform.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;

        var handRect = handTransform.GetComponent<RectTransform>();
        if (handRect == null) return;

        int cardCount = handTransform.childCount;
        if (cardCount == 0) return;

        var firstCard = handTransform.GetChild(0).GetComponent<RectTransform>();
        if (firstCard == null) return;

        // Visual width is what the player sees (rect * scale)
        float cardVisualWidth = firstCard.rect.width * firstCard.localScale.x;
        float handWidth = handRect.rect.width;

        if (cardCount == 1)
        {
            // Center single card
            firstCard.anchoredPosition = new Vector2(0, firstCard.anchoredPosition.y);
            return;
        }

        // Overlap scales with card count:
        //   2-3 cards:  5% overlap
        //   5 cards:   ~25% overlap
        //   7 cards:   ~40% overlap
        //  10 cards:   ~55% overlap
        float minOverlap = 0.05f;
        float maxOverlap = 0.65f;
        float t = Mathf.InverseLerp(2f, 10f, cardCount);
        float overlapPercent = Mathf.Lerp(minOverlap, maxOverlap, t);

        // Step = distance between left edges of consecutive cards
        float step = cardVisualWidth * (1f - overlapPercent);

        // Total visual width of all cards laid out
        float totalWidth = cardVisualWidth + step * (cardCount - 1);

        // Safety: if too wide, shrink step to fit
        if (totalWidth > handWidth)
        {
            step = (handWidth - cardVisualWidth) / (cardCount - 1);
            totalWidth = handWidth;
        }

        // Position each card centered in the hand
        // Hand pivot is (0.5, y), so x=0 is center of hand
        float startX = -totalWidth / 2f + cardVisualWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            var child = handTransform.GetChild(i);
            var cardRect = child.GetComponent<RectTransform>();
            if (cardRect == null) continue;

            float x = startX + step * i;
            Vector2 newPos = new Vector2(x, cardRect.anchoredPosition.y);

            // For cards still animating from the deck, only update the X of their
            // live target. The Y is mid-flight between deck and hand — using it would
            // corrupt the target and make the card stick out of the hand.
            var overlap = child.GetComponent<CardOverlapHandler>();
            if (overlap != null && overlap.isAnimating)
            {
                overlap.animTargetPos = new Vector2(x, overlap.animTargetPos.y);
            }
            else
            {
                cardRect.anchoredPosition = newPos;
            }
        }

        // Keep CardOverlapHandler rest positions in sync with the new layout
        CardOverlapHandler.SyncRestPositions(handTransform);
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

        // Refresh sorting orders so left cards render on top of right cards
        Transform ht = GameObject.Find("Hand").transform;
        if (ht != null)
            CardOverlapHandler.RefreshBaseSortingOrders(ht);
    }

    private void InitializeAllCardScripts()
    {
        Transform handTransform = GameObject.Find("Hand").transform;
        if (handTransform == null)
        {
            Debug.LogError("Hand UI panel not found in the scene.");
            return;
        }

        // Reset static hover state in case of scene reload (DontDestroyOnLoad persistence)
        CardOverlapHandler.ResetStaticState();

        foreach (Transform cardTransform in handTransform)
        {
            InitializeCardScripts(cardTransform.gameObject);
        }

        // Refresh sorting orders once after all cards are initialized
        CardOverlapHandler.RefreshBaseSortingOrders(handTransform);

        isHandInitialized = true;  //  Mark hand as initialized
        FindFirstObjectByType<BattleSystem>()?.StartEnemyAttacks();  //  Notify BattleSystem
    }


}
