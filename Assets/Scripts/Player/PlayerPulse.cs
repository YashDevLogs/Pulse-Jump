using System.Collections;
using UnityEngine;

public class PlayerPulse : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseScale = 1.5f;
    [SerializeField] private float pulseDuration = 0.3f;

    [Header("References")]
    [SerializeField] private PulseEnergy pulseEnergy;
    [SerializeField] private Shockwave shockwave;
    [SerializeField] private ParticleSystem pulseParticles;
    [SerializeField] private CameraShake cameraShake;


    private Vector3 baseScale;
    private Coroutine pulseCoroutine;

    public bool IsPulsing { get; private set; }


    private void Awake()
    {
        baseScale = transform.localScale;

        if (pulseEnergy == null)
            pulseEnergy = GetComponent<PulseEnergy>();
    }

    public void OnTap()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (pulseEnergy == null)
        {
            Debug.LogError("PlayerPulse: PulseEnergy reference is missing.");
            return;
        }

        if (!pulseEnergy.TryConsume())
        {
            Debug.Log("Pulse unavailable - energy is not full.");
            return;
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }


        if (shockwave != null)
        {
            shockwave.Launch();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayShockwave();
        }
        else
        {
            Debug.LogError("PlayerPulse: Shockwave reference is missing.");
        }

        if (pulseParticles != null)
        {
            pulseParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            pulseParticles.Play();
        }

        if (cameraShake != null)
        {
            cameraShake.Shake();
        }

        pulseCoroutine = StartCoroutine(PulseRoutine());
    }


    public void CancelPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        IsPulsing = false;

        transform.localScale = baseScale;
    }

    private IEnumerator PulseRoutine()
    {
        IsPulsing = true;

        float halfDuration = pulseDuration * 0.5f;

        yield return ScaleTo(
            baseScale * pulseScale,
            halfDuration
        );

        yield return ScaleTo(
            baseScale,
            halfDuration
        );

        IsPulsing = false;
        pulseCoroutine = null;
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }
}