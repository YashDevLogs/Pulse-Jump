using UnityEngine;

public class PulseGate : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float perfectDistance = 2.5f;
    [SerializeField] private float goodDistance = 5f;

    private bool playerHasCollided;
    private bool destroyed;

    private void OnTriggerEnter(Collider other)
    {
        if (playerHasCollided)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerHasCollided = true;

        Debug.Log("Player reached intact gate.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public PulseTiming EvaluateTiming(Vector3 playerPosition)
    {
        if (destroyed)
            return PulseTiming.None;

        float distance = Vector3.Distance(
            playerPosition,
            transform.position
        );

        Debug.Log($"Gate destroyed at {distance:F2}m");

        if (distance <= perfectDistance)
        {
            Debug.Log("PERFECT PULSE!");
            return PulseTiming.Perfect;
        }

        if (distance <= goodDistance)
        {
            Debug.Log("GOOD PULSE!");
            return PulseTiming.Good;
        }

        Debug.Log("EARLY PULSE!");
        return PulseTiming.Early;
    }

    public void ResetBarrier()
    {
        playerHasCollided = false;
    }
}