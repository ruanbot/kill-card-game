using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardUsage : MonoBehaviour, IPointerClickHandler
{
    private DisplayCard displayCard;
    private EnergyManager energyManager;
    private DiscardPileManager discardPileManager;
    private BattleSystem battleSystem;
    private PartyManager partyManager;
    private CardHover cardHover;
    // private PlayerDeck playerDeck;

    private bool isCardSelected = false;
    private bool isTargeting = false;

    private static CardUsage currentlySelectedCard;

    private void Start()
    {
        displayCard = GetComponent<DisplayCard>();
        energyManager = FindFirstObjectByType<EnergyManager>();
        discardPileManager = FindFirstObjectByType<DiscardPileManager>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
        cardHover = GetComponentInChildren<CardHover>();
        partyManager = FindFirstObjectByType<PartyManager>();
        // partyDeck = FindFirstObjectByType<PlayerDeck>();

        if (displayCard == null) Debug.LogWarning("DisplayCard component is missing on this GameObject.");
        if (energyManager == null) Debug.LogWarning("EnergyManager not found in the scene.");
        if (discardPileManager == null) Debug.LogWarning("DiscardPileManager not found in the scene.");
        if (battleSystem == null) Debug.LogWarning("BattleSystem not found in the scene.");
        if (cardHover == null) Debug.LogWarning("CardHover component is missing on this GameObject.");
        if (partyManager == null) Debug.LogWarning("PartyManager not found in the scene.");
        // if (playerDeck == null) Debug.LogWarning("PlayerDeck not found in the scene");
    }

    private void Update()
    {
        if (isTargeting && Input.GetMouseButtonDown(0) && currentlySelectedCard == this)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");

                // Check if the raycast hit a valid target with BattleVisuals component
                BattleVisuals hitBattleVisuals = hit.collider.GetComponentInParent<BattleVisuals>();
                if (hitBattleVisuals != null)
                {
                    Debug.Log($"{hit.collider.gameObject.name} has BattleVisuals component, invoking TargetSelected event.");
                    hitBattleVisuals.SubscribeToTargetSelected(OnTargetSelected);

                }
                else
                {
                    Debug.LogWarning($"{hit.collider.gameObject.name} does not have BattleVisuals component.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast did not hit any object.");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCardSelected)
        {
            DeselectCard();
        }
        else
        {
            if (currentlySelectedCard != null && currentlySelectedCard != this)
            {
                currentlySelectedCard.DeselectCard();
            }
            SelectCard();
        }
    }

    private void SelectCard()
    {
        if (displayCard != null && displayCard.card != null && energyManager != null)
        {
            int manaCost = displayCard.card.manaCost;

            if (energyManager.HasEnoughEnergy(manaCost))
            {
                isCardSelected = true;
                currentlySelectedCard = this;
                isTargeting = displayCard.card.targetType != TargetType.Self;

                if (cardHover != null)
                {
                    cardHover.SetHover(true);
                }

                if (displayCard.card.targetType == TargetType.Self)
                {
                    // Automatically target the default party member for Self-target
                    List<PartyMember> party = partyManager.GetCurrentParty();
                    if (party.Count > 0)
                    {
                        PartyMember defaultTarget = party[0];
                        UseCardEffect(displayCard.card, defaultTarget);
                        SendToDiscardPile();
                    }
                    else
                    {
                        Debug.LogWarning("No party members available for Self-target.");
                    }
                }
                else if (displayCard.card.targetType == TargetType.Enemy)
                {
                    // Automatically target the only enemy if there's just one
                    if (battleSystem.enemyBattlers.Count == 1)
                    {
                        BattleEntities enemyEntity = battleSystem.enemyBattlers[0];
                        UseCardEffect(displayCard.card, enemyEntity);
                        SendToDiscardPile();
                    }
                    else
                    {
                        Debug.Log("Select a target.");
                    }
                }
            }
            else
            {
                Debug.Log("Not enough energy to use this card!");
            }
        }
    }


    private void DeselectCard()
    {
        isCardSelected = false;
        currentlySelectedCard = null;

        if (cardHover != null)
        {
            cardHover.SetHover(false);
        }

        isTargeting = false;
        Debug.Log("Card deselected.");
    }

    private void HandleTargetSelection()
    {
        Debug.Log("HandleTargetSelection called.");
        TargetType targetType = displayCard.card.targetType;

        if (targetType == TargetType.Friendly)
        {
            List<BattleEntities> partyMembers = battleSystem.playerBattlers;
            EnableTargetSelection(partyMembers);
        }
        else if (targetType == TargetType.Enemy)
        {
            List<BattleEntities> enemies = battleSystem.enemyBattlers;
            EnableTargetSelection(enemies);
        }
        else
        {
            Debug.LogWarning("Invalid target type for card.");
        }
    }

    private void EnableTargetSelection(List<BattleEntities> targets)
    {
        foreach (var target in targets)
        {
            // Use SubscribeToTargetSelected to add the listener without accessing the private TargetSelected event directly
            target.BattleVisuals.SubscribeToTargetSelected(OnTargetSelected);
        }
    }



    private void OnTargetSelected(BattleVisuals targetVisual)
    {
        var target = battleSystem.allBattlers.Find(entity => entity.BattleVisuals == targetVisual);

        if (target != null)
        {
            Debug.Log($"{displayCard.card.cardName} effect applied to {target.Name}");
            UseCardEffect(displayCard.card, target);
            SendToDiscardPile();

            foreach (var entity in battleSystem.allBattlers)
            {
                entity.BattleVisuals.DisableTargetHighlight();
            }
        }
        else
        {
            Debug.LogWarning("Target not found in allBattlers.");
        }

        isTargeting = false;
    }


    private void UseCardEffect(Card cardData, object target)
    {
        if (cardData.targetType == TargetType.Enemy && target is BattleEntities enemyEntity)
        {
            if (cardData.power > 0)
            {
                Debug.Log($"{cardData.cardName} deals {cardData.power} damage to {enemyEntity.Name}!");
                TakeDamage(enemyEntity, cardData.power);
                
                if (enemyEntity.BattleVisuals != null)
                {
                    enemyEntity.BattleVisuals.PlayHitAnimation();
                }
            }
        }
        else if (cardData.targetType == TargetType.Self && target is BattleEntities partyEntity)
        {
            Debug.Log($"{cardData.cardName} effect applied to {partyEntity.Name}");
            TakeDamage(partyEntity, cardData.power);

            if (partyEntity.BattleVisuals != null)
                {
                    partyEntity.BattleVisuals.PlayHitAnimation();
                }
        }

        if (cardData.drawXcards > 0)
        {
            Debug.Log($"{cardData.cardName} draws {cardData.drawXcards} cards.");
        }
    }

    private void SendToDiscardPile()
    {
        energyManager.SpendEnergy(displayCard.card.manaCost);
        discardPileManager.AddToDiscardPile(gameObject);
        Debug.Log($"{displayCard.card.cardName} was used and sent to the discard pile!");

        DeselectCard();
    }

private void TakeDamage(BattleEntities entity, int damage)
{
    entity.CurrentHealth -= damage;
    entity.CurrentHealth = Mathf.Clamp(entity.CurrentHealth, 0, entity.MaxHealth);
    Debug.Log($"Applied {damage} damage to {entity.Name}. New health: {entity.CurrentHealth}");

    if (entity.BattleVisuals != null)
    {
        // Update the visuals to reflect the new health
        entity.BattleVisuals.SetHealthValues(entity.CurrentHealth, entity.MaxHealth);
    }

    if (entity.CurrentHealth <= 0)
    {
        entity.BattleVisuals.PlayDeathAnimation();
    }
}



    // private void ApplyDamageToEnemy(BattleEntities enemyEntity, int damage)
    // {
    //     enemyEntity.CurrentHealth -= damage;
    //     enemyEntity.CurrentHealth = Mathf.Clamp(enemyEntity.CurrentHealth, 0, enemyEntity.MaxHealth);
    //     Debug.Log($"Applied {damage} damage to {enemyEntity.Name}. New health: {enemyEntity.CurrentHealth}");

    //     if (enemyEntity.BattleVisuals != null)
    //     {
    //         // Update the visuals to reflect the new health
    //         enemyEntity.BattleVisuals.SetHealthValues(enemyEntity.CurrentHealth, enemyEntity.MaxHealth);
    //     }
    // }

    // private void ApplyDamageToPartyMember(BattleEntities partyEntity, int damage)
    // {
    //     partyEntity.CurrentHealth -= damage;
    //     partyEntity.CurrentHealth = Mathf.Clamp(partyEntity.CurrentHealth, 0, partyEntity.MaxHealth);
    //     Debug.Log($"Applied {damage} damage to {partyEntity.Name}. New health: {partyEntity.CurrentHealth}");

    //     if (partyEntity.BattleVisuals != null)
    //     {
    //         // Update the visuals to reflect the new health
    //         partyEntity.BattleVisuals.SetHealthValues(partyEntity.CurrentHealth, partyEntity.MaxHealth);
    //     }
    // }



}
