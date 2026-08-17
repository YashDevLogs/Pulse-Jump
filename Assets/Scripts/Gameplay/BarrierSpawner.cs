using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BarrierPool barrierPool;
    [SerializeField] private Transform player;

    [Header("Initial Layout")]
    [SerializeField] private int initialBarrierCount = 6;
    [SerializeField] private float firstSpawnDistance = 45f;

    [Header("Recycling")]
    [SerializeField] private float recycleDistance = 30f;
    [SerializeField] private float recycleDelay = 1.5f;

    private readonly List<GameObject> activeBarriers = new();

    private bool initialized;

    private void Start()
    {
        if (barrierPool == null)
        {
            Debug.LogError("BarrierSpawner: Barrier Pool is missing.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("BarrierSpawner: Player reference is missing.");
            return;
        }

        SpawnInitialBarriers();

        initialized = true;
    }

    private void Update()
    {

        if (GameManager.Instance != null &&
    GameManager.Instance.CurrentState != GameState.Playing)
            return;
        if (!initialized)
            return;

        CheckForPassedBarriers();
    }

    private void SpawnInitialBarriers()
    {
        float spawnZ =
            player.position.z + firstSpawnDistance;

        for (int i = 0; i < initialBarrierCount; i++)
        {
            GameObject barrier =
                barrierPool.GetRandomBarrier();

            if (barrier == null)
            {
                Debug.LogWarning(
                    "BarrierSpawner: Pool ran out of barriers."
                );

                break;
            }

            PlaceBarrier(barrier, spawnZ);

            activeBarriers.Add(barrier);

            float minSpacing = DifficultyManager.Instance != null
    ? DifficultyManager.Instance.CurrentMinSpacing
    : 40f;

            float maxSpacing = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.CurrentMaxSpacing
                : 60f;

            float spacing = Random.Range(
                minSpacing,
                maxSpacing
            );

            spawnZ += spacing;
        }

        Debug.Log(
            $"BarrierSpawner: Spawned {activeBarriers.Count} initial barriers."
        );
    }

    private void PlaceBarrier(
        GameObject barrier,
        float spawnZ)
    {
        Vector3 position = barrier.transform.position;

        position.x = player.position.x;
        position.z = spawnZ;

        barrier.transform.position = position;

        barrier.SetActive(true);
    }

    private void CheckForPassedBarriers()
    {
        for (int i = activeBarriers.Count - 1; i >= 0; i--)
        {
            GameObject barrier = activeBarriers[i];

            if (barrier == null)
                continue;

            float distanceBehindPlayer =
                player.position.z -
                barrier.transform.position.z;

            if (distanceBehindPlayer >= recycleDistance)
            {
                StartCoroutine(
                    RecycleBarrierAfterDelay(barrier)
                );

                activeBarriers.RemoveAt(i);
            }
        }
    }

    private IEnumerator RecycleBarrierAfterDelay(
        GameObject barrier)
    {
        yield return new WaitForSeconds(recycleDelay);

        if (barrier == null)
            yield break;

        // Reset the barrier before reusing it.
        barrierPool.ReturnBarrier(barrier);

        // Find the barrier currently farthest ahead.
        float farthestZ = GetFarthestBarrierZ();

        float minSpacing = DifficultyManager.Instance != null
    ? DifficultyManager.Instance.CurrentMinSpacing
    : 40f;

        float maxSpacing = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentMaxSpacing
            : 60f;

        float spacing = Random.Range(
            minSpacing,
            maxSpacing
        );

        float newZ = farthestZ + spacing;

        Vector3 position = barrier.transform.position;

        position.x = player.position.x;
        position.z = newZ;

        barrier.transform.position = position;

        barrier.SetActive(true);

        activeBarriers.Add(barrier);

        Debug.Log(
            $"Barrier reused at Z = {newZ:F1}"
        );
    }

    private float GetFarthestBarrierZ()
    {
        float farthestZ = player.position.z;

        foreach (GameObject barrier in activeBarriers)
        {
            if (barrier == null)
                continue;

            if (barrier.transform.position.z > farthestZ)
            {
                farthestZ =
                    barrier.transform.position.z;
            }
        }

        return farthestZ;
    }
}