using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Visual Movement")]
[SerializeField] private float rollSpeedMultiplier = 1f;

private void Update()
{
    RotateVisual();
}

private void RotateVisual()
{
    if (WorldMovement.Instance == null)
        return;

    if (GameManager.Instance != null &&
        GameManager.Instance.CurrentState != GameState.Playing)
        return;

    float rotationSpeed =
        WorldMovement.Instance.CurrentSpeed * 30f;

    transform.Rotate(
        Vector3.right,
        rotationSpeed * Time.deltaTime,
        Space.Self
    );
}
    public void StopMovement()
    {
        // Player remains visually anchored.
    }
}