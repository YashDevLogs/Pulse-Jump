using System.Collections.Generic;
using UnityEngine;

public class BarrierPool : MonoBehaviour
{
    [System.Serializable]
    private class BarrierPoolEntry
    {
        public GameObject prefab;
        public int amount = 3;
    }

    [SerializeField] private BarrierPoolEntry[] poolEntries;

    private readonly List<GameObject> availableBarriers = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        availableBarriers.Clear();

        foreach (BarrierPoolEntry entry in poolEntries)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning("BarrierPool: Missing prefab.");
                continue;
            }

            for (int i = 0; i < entry.amount; i++)
            {
                GameObject barrier = Instantiate(
                    entry.prefab,
                    transform
                );

                barrier.name = $"{entry.prefab.name}_Pool_{i + 1}";

                barrier.SetActive(false);

                availableBarriers.Add(barrier);
            }
        }

        Debug.Log(
            $"BarrierPool: Created {availableBarriers.Count} barriers."
        );
    }

    public GameObject GetRandomBarrier()
    {
        if (availableBarriers.Count == 0)
        {
            Debug.LogWarning("BarrierPool: No available barriers.");
            return null;
        }

        int randomIndex = Random.Range(0, availableBarriers.Count);

        GameObject barrier = availableBarriers[randomIndex];

        availableBarriers.RemoveAt(randomIndex);

        return barrier;
    }

    public void ReturnBarrier(GameObject barrier)
    {
        if (barrier == null)
            return;

        DestructibleProp destructible =
            barrier.GetComponent<DestructibleProp>();

        if (destructible != null)
        {
            destructible.ResetDestruction();
        }

        barrier.SetActive(false);

        barrier.transform.SetParent(transform);

        availableBarriers.Add(barrier);
    }
}