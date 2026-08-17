using UnityEngine;

public class PulseEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float rechargeTime = 3f;

    public float CurrentEnergy { get; private set; }
    public bool IsReady => CurrentEnergy >= 0.999f;

    private void Start()
    {
        CurrentEnergy = 1f;
    }

    private bool wasReady;

    private void Update()
    {
        // Don't recharge while paused or game over
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Recharge
        if (!IsReady)
        {
            CurrentEnergy += Time.deltaTime / rechargeTime;
            CurrentEnergy = Mathf.Clamp01(CurrentEnergy);
        }

        // Play sound once when energy becomes full
        if (IsReady && !wasReady)
        {
            wasReady = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPulseReady();
            }
        }

        // Allow the sound to trigger again after pulse is consumed
        if (!IsReady)
        {
            wasReady = false;
        }
    }

    public bool TryConsume()
    {
        if (!IsReady)
            return false;

        CurrentEnergy = 0f;
        return true;
    }
}