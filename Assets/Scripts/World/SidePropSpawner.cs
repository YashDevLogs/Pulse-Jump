using System.Collections.Generic;
using UnityEngine;

public class SidePropSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Prop Prefabs")]
    [SerializeField] private GameObject[] propPrefabs;

    [Header("Pool")]
    [SerializeField] private int poolSize = 16;

    [Header("X Position")]
    [SerializeField] private float minSideX = 2.5f;
    [SerializeField] private float maxSideX = 5f;

    [Header("Y Position")]
    [SerializeField] private float spawnY = 0.39f;

    [Header("Z Spacing")]
    [SerializeField] private float firstSpawnDistance = 20f;
    [SerializeField] private float minSpacing = 12f;
    [SerializeField] private float maxSpacing = 25f;

    [Header("Recycling")]
    [SerializeField] private float recycleDistance = 25f;

    private readonly List<GameObject> activeProps = new();

    private float nextSpawnZ;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("SidePropSpawner: Player reference missing.");
            return;
        }

        if (propPrefabs == null || propPrefabs.Length == 0)
        {
            Debug.LogError("SidePropSpawner: No prop prefabs assigned.");
            return;
        }

        CreateInitialProps();
    }

    private void Update()
    {

        if (GameManager.Instance != null &&
    GameManager.Instance.CurrentState != GameState.Playing)
            return;
            
        RecyclePassedProps();
    }

    private void CreateInitialProps()
    {
        nextSpawnZ =
            player.position.z + firstSpawnDistance;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prop = CreateProp();

            if (prop == null)
                continue;

            PlaceProp(prop, nextSpawnZ);

            activeProps.Add(prop);

            nextSpawnZ +=
                Random.Range(minSpacing, maxSpacing);
        }

        Debug.Log(
            $"SidePropSpawner: Created {activeProps.Count} props."
        );
    }

    private GameObject CreateProp()
    {
        GameObject prefab =
            propPrefabs[
                Random.Range(0, propPrefabs.Length)
            ];

        if (prefab == null)
            return null;

        GameObject prop =
            Instantiate(prefab, transform);

        prop.SetActive(true);

        return prop;
    }

    private void PlaceProp(
        GameObject prop,
        float spawnZ)
    {
        float sideX = GetRandomSideX();

        Vector3 position =
            prop.transform.position;

        position.x = sideX;
        position.y = spawnY;
        position.z = spawnZ;

        prop.transform.position = position;

        // Randomly rotate decorative props.
        prop.transform.rotation =
            Quaternion.Euler(
                0f,
                Random.Range(0f, 360f),
                0f
            );

        prop.SetActive(true);
    }

    private float GetRandomSideX()
    {
        bool leftSide =
            Random.value < 0.5f;

        float x =
            Random.Range(
                minSideX,
                maxSideX
            );

        return leftSide ? -x : x;
    }

    private void RecyclePassedProps()
    {
        for (int i = activeProps.Count - 1; i >= 0; i--)
        {
            GameObject prop =
                activeProps[i];

            if (prop == null)
            {
                activeProps.RemoveAt(i);
                continue;
            }

            float distanceBehindPlayer =
                player.position.z -
                prop.transform.position.z;

            if (distanceBehindPlayer >= recycleDistance)
            {
                RepositionProp(prop);

                activeProps.RemoveAt(i);
                activeProps.Add(prop);
            }
        }
    }

    private void RepositionProp(GameObject prop)
    {
        float farthestZ =
            GetFarthestPropZ();

        float spacing =
            Random.Range(
                minSpacing,
                maxSpacing
            );

        PlaceProp(
            prop,
            farthestZ + spacing
        );
    }

    private float GetFarthestPropZ()
    {
        float farthestZ =
            player.position.z;

        foreach (GameObject prop in activeProps)
        {
            if (prop == null)
                continue;

            if (prop.transform.position.z > farthestZ)
            {
                farthestZ =
                    prop.transform.position.z;
            }
        }

        return farthestZ;
    }
}