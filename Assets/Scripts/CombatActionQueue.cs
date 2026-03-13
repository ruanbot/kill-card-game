using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAction
{
    public string description;
    public float enqueuedTime;
    public bool isPlayerAction;
    public Action<Action> execute; // receives a "done" callback
}

public class CombatActionQueue : MonoBehaviour
{
    private readonly List<CombatAction> queue = new List<CombatAction>();
    private bool isProcessing;
    public bool IsProcessing => isProcessing;

    private CombatAction currentAction;

    private EnergyManager energyManager;
    private CardManager cardManager;

    private void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        cardManager = CardManager.Instance;
    }

    /// <summary>
    /// Enqueue a combat action. Uses sorted insert so same-frame actions
    /// resolve player-first (FIFO within same priority).
    /// </summary>
    public void Enqueue(CombatAction action)
    {
        // Sorted insert: player actions before enemy actions at the same enqueue time
        int insertIndex = queue.Count;
        for (int i = 0; i < queue.Count; i++)
        {
            if (action.enqueuedTime < queue[i].enqueuedTime)
            {
                insertIndex = i;
                break;
            }
            if (Mathf.Approximately(action.enqueuedTime, queue[i].enqueuedTime)
                && action.isPlayerAction && !queue[i].isPlayerAction)
            {
                insertIndex = i;
                break;
            }
        }
        queue.Insert(insertIndex, action);
        UpdateCardPlayability();

        if (!isProcessing)
        {
            // Set flag BEFORE starting coroutine to prevent duplicate ProcessQueue starts
            isProcessing = true;
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        // isProcessing is already set true by Enqueue() before this coroutine starts
        if (energyManager != null)
            energyManager.PauseRegen();
        UpdateCardPlayability();

        while (queue.Count > 0)
        {
            currentAction = queue[0];
            queue.RemoveAt(0);
            UpdateCardPlayability();

            bool done = false;
            float timeout = 5f;

            Debug.Log($"[CombatQueue] Executing: {currentAction.description}");
            currentAction.execute(() => done = true);

            // Wait for the action's done callback or timeout
            float elapsed = 0f;
            while (!done && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!done)
            {
                Debug.LogWarning($"[CombatQueue] Action '{currentAction.description}' timed out after {timeout}s");
            }

            currentAction = null;
            UpdateCardPlayability();

            // Gap between actions so they don't visually blend together
            yield return new WaitForSeconds(0.25f);
        }

        isProcessing = false;
        if (energyManager != null)
            energyManager.ResumeRegen();
        if (cardManager != null)
            cardManager.SetCardsPlayable(true);
    }

    private bool HasPlayerAction()
    {
        if (currentAction != null && currentAction.isPlayerAction)
            return true;

        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].isPlayerAction)
                return true;
        }

        return false;
    }

    private void UpdateCardPlayability()
    {
        if (cardManager != null)
            cardManager.SetCardsPlayable(!HasPlayerAction());
    }

    private void OnDestroy()
    {
        queue.Clear();
        StopAllCoroutines();
    }
}
