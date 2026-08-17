using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PerfectImpactPostProcess : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume volume;

    [Header("Perfect Impact")]
    [SerializeField] private float effectInDuration = 0.12f;
    [SerializeField] private float holdDuration = 0.20f;
    [SerializeField] private float effectOutDuration = 0.40f;

    [Header("Bloom")]
    [SerializeField] private float impactBloomIntensity = 1.0f;

    [Header("Chromatic Aberration")]
    [SerializeField] private float impactChromaticIntensity = 1.0f;

    [Header("Vignette")]
    [SerializeField] private float impactVignetteIntensity = 0.18f;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private float baseBloomIntensity;
    private float baseChromaticIntensity;
    private float baseVignetteIntensity;

    private Coroutine effectCoroutine;

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        if (volume == null)
        {
            Debug.LogError(
                "PerfectImpactPostProcess: Volume reference is missing."
            );

            enabled = false;
            return;
        }

        VolumeProfile profile = volume.profile;

        if (profile == null)
        {
            Debug.LogError(
                "PerfectImpactPostProcess: Volume Profile is missing."
            );

            enabled = false;
            return;
        }

        profile.TryGet(out bloom);
        profile.TryGet(out chromaticAberration);
        profile.TryGet(out vignette);

        if (bloom != null)
        {
            baseBloomIntensity =
                bloom.intensity.value;
        }

        if (chromaticAberration != null)
        {
            baseChromaticIntensity =
                chromaticAberration.intensity.value;
        }

        if (vignette != null)
        {
            baseVignetteIntensity =
                vignette.intensity.value;
        }

        Debug.Log(
            "PerfectImpactPostProcess initialized."
        );

        Debug.Log(
            $"Base Bloom: {baseBloomIntensity}"
        );

        Debug.Log(
            $"Base Chromatic: {baseChromaticIntensity}"
        );

        Debug.Log(
            $"Base Vignette: {baseVignetteIntensity}"
        );
    }

    private void Start()
    {
        StartCoroutine(
            SubscribeWhenReady()
        );
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (ScoreManager.Instance == null)
        {
            yield return null;
        }

        ScoreManager.Instance.OnPerfectCinematic -=
            PlayPerfectImpactEffect;

        ScoreManager.Instance.OnPerfectCinematic +=
            PlayPerfectImpactEffect;

        Debug.Log(
            "PerfectImpactPostProcess subscribed to ScoreManager."
        );
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPerfectCinematic -=
                PlayPerfectImpactEffect;
        }
    }

    private void PlayPerfectImpactEffect()
    {
        Debug.Log(
            "PERFECT POST PROCESS START"
        );

        if (effectCoroutine != null)
        {
            StopCoroutine(
                effectCoroutine
            );
        }

        effectCoroutine =
            StartCoroutine(
                ImpactRoutine()
            );
    }

    private IEnumerator ImpactRoutine()
    {
        // ==================================================
        // EFFECT IN
        // ==================================================

        yield return AnimateEffect(
            0f,
            1f,
            effectInDuration
        );

        // ==================================================
        // FORCE PEAK
        // ==================================================

        ApplyEffect(1f);

        Debug.Log(
            $"PP PEAK | " +
            $"Bloom: {GetBloomValue()} | " +
            $"Chromatic: {GetChromaticValue()} | " +
            $"Vignette: {GetVignetteValue()}"
        );

        // ==================================================
        // HOLD PEAK
        // ==================================================

        yield return new WaitForSecondsRealtime(
            holdDuration
        );

        // ==================================================
        // EFFECT OUT
        // ==================================================

        yield return AnimateEffect(
            1f,
            0f,
            effectOutDuration
        );

        // ==================================================
        // RESTORE
        // ==================================================

        RestoreBaseValues();

        Debug.Log(
            "PERFECT POST PROCESS END"
        );

        effectCoroutine = null;
    }

    private IEnumerator AnimateEffect(
        float start,
        float end,
        float duration)
    {
        if (duration <= 0f)
        {
            ApplyEffect(end);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            float amount =
                Mathf.Lerp(
                    start,
                    end,
                    t
                );

            ApplyEffect(amount);

            yield return null;
        }

        ApplyEffect(end);
    }

    private void ApplyEffect(float amount)
    {
        if (bloom != null)
        {
            bloom.intensity.value =
                Mathf.Lerp(
                    baseBloomIntensity,
                    impactBloomIntensity,
                    amount
                );
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value =
                Mathf.Lerp(
                    baseChromaticIntensity,
                    impactChromaticIntensity,
                    amount
                );
        }

        if (vignette != null)
        {
            vignette.intensity.value =
                Mathf.Lerp(
                    baseVignetteIntensity,
                    impactVignetteIntensity,
                    amount
                );
        }
    }

    private void RestoreBaseValues()
    {
        if (bloom != null)
        {
            bloom.intensity.value =
                baseBloomIntensity;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value =
                baseChromaticIntensity;
        }

        if (vignette != null)
        {
            vignette.intensity.value =
                baseVignetteIntensity;
        }
    }

    private float GetBloomValue()
    {
        return bloom != null
            ? bloom.intensity.value
            : -1f;
    }

    private float GetChromaticValue()
    {
        return chromaticAberration != null
            ? chromaticAberration.intensity.value
            : -1f;
    }

    private float GetVignetteValue()
    {
        return vignette != null
            ? vignette.intensity.value
            : -1f;
    }
    public void PlayImpact()
    {
        PlayPerfectImpactEffect();
    }
}