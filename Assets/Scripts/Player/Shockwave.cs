using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    [Header("Expansion")]
    [SerializeField] private float expansionSpeed = 12f;
    [SerializeField] private float maxRadius = 2f;

    [Header("References")]
    [SerializeField] private Transform visual;
    [SerializeField] private SphereCollider shockwaveCollider;
    [SerializeField] private Transform playerTransform;

    [Header("Visual Wobble")]
    [SerializeField] private float wobbleAmount = 0.18f;
    [SerializeField] private float wobbleFrequency = 4f;
    [SerializeField] private float wobbleDamping = 4f;

    [Header("Fade Out")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    private float currentRadius;
    private bool isActive;
    private bool isFading;
    private float fadeTimer;

    private Vector3 startLocalPosition;

    // Prevents multiple colliders belonging to the same
    // barrier from triggering the shockwave twice.
    private readonly HashSet<DestructibleProp> processedBarriers =
        new HashSet<DestructibleProp>();

    private void Awake()
    {
        startLocalPosition = transform.localPosition;

        if (shockwaveCollider == null)
        {
            shockwaveCollider = GetComponent<SphereCollider>();
        }

        gameObject.SetActive(false);
    }

    public void Launch()
    {
        transform.localPosition = startLocalPosition;

        currentRadius = 0f;
        isActive = true;
        isFading = false;
        fadeTimer = 0f;

        // New shockwave = new detection cycle.
        processedBarriers.Clear();

        transform.localScale = Vector3.one;

        if (visual != null)
        {
            visual.localScale = Vector3.zero;
        }

        if (shockwaveCollider != null)
        {
            shockwaveCollider.enabled = true;
            shockwaveCollider.radius = 0.01f;
        }

        gameObject.SetActive(true);

        Debug.Log("Shockwave launched.");
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
        GameManager.Instance.CurrentState != GameState.Playing)
            return;


        if (!isActive)
            return;

        // ==================================================
        // EXPANSION
        // ==================================================

        if (!isFading)
        {
            currentRadius +=
                expansionSpeed * Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    currentRadius / maxRadius
                );

            float baseScale =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    normalized
                );

            float wobble =
                Mathf.Sin(
                    normalized *
                    Mathf.PI *
                    wobbleFrequency
                )
                * wobbleAmount
                * Mathf.Exp(
                    -normalized *
                    wobbleDamping
                );

            float visualScale =
                baseScale *
                (1f + wobble);

            if (visual != null)
            {
                visual.localScale =
                    Vector3.one *
                    visualScale *
                    maxRadius;
            }

            if (shockwaveCollider != null)
            {
                shockwaveCollider.radius =
                    currentRadius;
            }

            if (currentRadius >= maxRadius)
            {
                currentRadius = maxRadius;

                isFading = true;
                fadeTimer = 0f;

                if (shockwaveCollider != null)
                {
                    shockwaveCollider.enabled = false;
                }
            }

            return;
        }

        // ==================================================
        // FADE
        // ==================================================

        fadeTimer += Time.deltaTime;

        float fadeProgress =
            Mathf.Clamp01(
                fadeTimer / fadeOutDuration
            );

        float fadeScale =
            Mathf.SmoothStep(
                1f,
                0f,
                fadeProgress
            );

        if (visual != null)
        {
            visual.localScale =
                Vector3.one *
                maxRadius *
                fadeScale;
        }

        if (fadeProgress >= 1f)
        {
            StopShockwave();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null &&
             GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (!isActive)
            return;

        DestructibleProp destructible =
            other.GetComponentInParent<DestructibleProp>();

        if (destructible == null)
            return;

        // ==================================================
        // IMPORTANT
        // ==================================================
        // A barrier can have multiple colliders.
        // Only process that barrier once per shockwave.
        // ==================================================

        if (processedBarriers.Contains(destructible))
        {
            Debug.Log(
                "Shockwave ignored duplicate collider on: " +
                destructible.name
            );

            return;
        }

        processedBarriers.Add(destructible);

        // ==================================================
        // EVALUATE TIMING
        // ==================================================

        PulseGate gate =
            destructible.GetComponent<PulseGate>();

        PulseTiming timing =
            PulseTiming.Good;

        if (gate != null)
        {
            timing =
                gate.EvaluateTiming(
                    playerTransform.position
                );
        }

        Debug.Log(
            "Pulse result: " +
            timing
        );

        // ==================================================
        // PERFECT
        // ==================================================

        if (timing == PulseTiming.Perfect)
        {
            bool shouldPlayCinematic = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPerfect();
            }

            if (ScoreManager.Instance != null)
            {
                int nextMultiplier =
                    ScoreManager.Instance.PerfectMultiplier + 1;

                shouldPlayCinematic =
                    nextMultiplier == 1 ||
                    (
                        nextMultiplier >= 5 &&
                        nextMultiplier % 5 == 0
                    );

                Debug.Log(
                    $"Perfect detected. " +
                    $"Next multiplier: {nextMultiplier} | " +
                    $"Cinematic: {shouldPlayCinematic}"
                );
            }

            if (shouldPlayCinematic)
            {
                // Prevent the player from hitting the
                // intact barrier while the cinematic plays.
                destructible.PrepareForPerfectCinematic();

                StartPerfectImpact(
                    destructible,
                    timing
                );
            }
            else
            {
                // Normal Perfect — no cinematic.
                destructible.DestroyProp(
                    transform.position,
                    playerTransform.position,
                    timing
                );
            }

            return;
        }

        // ==================================================
        // GOOD / EARLY
        // ==================================================

        destructible.DestroyProp(
            transform.position,
            playerTransform.position
        );
    }

    private void StartPerfectImpact(
        DestructibleProp destructible,
        PulseTiming timing)
    {
        PerfectCinematicController cinematic =
            FindFirstObjectByType<
                PerfectCinematicController>();

        if (cinematic == null)
        {
            destructible.DestroyProp(
                transform.position,
                playerTransform.position,
                timing
            );

            return;
        }

        cinematic.PlayPerfectImpact(
            () =>
            {
                destructible.DestroyProp(
                    transform.position,
                    playerTransform.position,
                    timing
                );
            }
        );
    }

    public void Cancel()
    {
        isActive = false;
        isFading = false;

        if (shockwaveCollider != null)
            shockwaveCollider.enabled = false;

        if (visual != null)
            visual.localScale = Vector3.zero;

        gameObject.SetActive(false);
    }

    private void StopShockwave()
    {
        isActive = false;
        isFading = false;

        if (shockwaveCollider != null)
        {
            shockwaveCollider.enabled = false;
        }

        gameObject.SetActive(false);
    }
}