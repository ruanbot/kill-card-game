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
        PauseSystems();

        while (queue.Count > 0)
        {
            var action = queue[0];
            queue.RemoveAt(0);

            bool done = false;
            float timeout = 5f;

            Debug.Log($"[CombatQueue] Executing: {action.description}");
            action.execute(() => done = true);

            // Wait for the action's done callback or timeout
            float elapsed = 0f;
            while (!done && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!done)
            {
                Debug.LogWarning($"[CombatQueue] Action '{action.description}' timed out after {timeout}s");
            }

            // Gap between actions so they don't visually blend together
            yield return new WaitForSeconds(0.25f);
        }

        isProcessing = false;
        ResumeSystems();
    }

    private void PauseSystems()
    {
        if (energyManager != null)
            energyManager.PauseRegen();
        if (cardManager != null)
            cardManager.SetCardsPlayable(false);
    }

    private void ResumeSystems()
    {
        if (energyManager != null)
            energyManager.ResumeRegen();
        if (cardManager != null)
            cardManager.SetCardsPlayable(true);
    }

    private void OnDestroy()
    {
        queue.Clear();
        StopAllCoroutines();
    }
}
