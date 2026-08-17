using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Barrier Score")]
    [SerializeField] private int goodScore = 100;
    [SerializeField] private int perfectBaseScore = 250;

    [Header("Distance Score")]
    [SerializeField] private float distanceMilestone = 100f;
    [SerializeField] private int distanceScore = 100;

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    public float DistanceTravelled { get; private set; }

    public int CurrentDistanceMilestone { get; private set; }

    public int PerfectMultiplier { get; private set; } = 0;

    private const string HighScoreKey = "HighScore";

    private float distanceAccumulator;

    public event Action<int> OnDistanceMilestone;
    public event Action<PulseTiming, int, int> OnTimingResult;
    public event Action<int> OnScoreChanged;

    public Action OnPerfectCinematic;
    public Action OnPerfectImpact;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HighScore = PlayerPrefs.GetInt(
            HighScoreKey,
            0
        );
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (WorldMovement.Instance == null)
            return;

        float distanceThisFrame =
            WorldMovement.Instance.CurrentSpeed *
            Time.deltaTime;

        AddDistance(distanceThisFrame);
    }

    private void AddDistance(float distance)
    {
        DistanceTravelled += distance;
        distanceAccumulator += distance;

        while (distanceAccumulator >= distanceMilestone)
        {
            distanceAccumulator -= distanceMilestone;

            CurrentDistanceMilestone += 1;

            AddScore(distanceScore);

            int milestoneDistance =
                CurrentDistanceMilestone *
                Mathf.RoundToInt(distanceMilestone);

            OnDistanceMilestone?.Invoke(
                milestoneDistance
            );
        }
    }

    public bool ShouldPlayPerfectCinematic()
    {
        if (PerfectMultiplier == 1)
            return true;

        return PerfectMultiplier >= 5 &&
               PerfectMultiplier % 5 == 0;
    }

    public void AddBarrierScore(
        PulseTiming timing,
        bool triggerCinematic = true)
    {
        int points = 0;

        switch (timing)
        {
            case PulseTiming.Early:

                ResetPerfectCombo();

                points = 0;
                break;


            case PulseTiming.Good:

                ResetPerfectCombo();

                points = goodScore;
                break;


            case PulseTiming.Perfect:

                PerfectMultiplier++;

                points =
                    perfectBaseScore *
                    PerfectMultiplier;

                // --------------------------------------------------
                // PERFECT IMPACT FX
                // --------------------------------------------------

                if (ShouldPlayPerfectCinematic())
                {
                    Debug.Log(
                        $"PERFECT IMPACT FX TRIGGERED at " +
                        $"{PerfectMultiplier}x"
                    );

                    OnPerfectImpact?.Invoke();
                }

                // --------------------------------------------------
                // CINEMATIC EVENT
                // --------------------------------------------------
                // Keep the existing cinematic event behaviour
                // untouched. Shockwave currently controls the
                // actual camera cinematic directly.

                if (triggerCinematic &&
                    ShouldPlayPerfectCinematic())
                {
                    Debug.Log(
                        $"PERFECT CINEMATIC EVENT TRIGGERED at " +
                        $"{PerfectMultiplier}x"
                    );

                    OnPerfectCinematic?.Invoke();
                }

                break;
        }

        AddScore(points);

        OnTimingResult?.Invoke(
            timing,
            points,
            PerfectMultiplier
        );

        Debug.Log(
            $"Timing: {timing} | " +
            $"Points: {points} | " +
            $"Multiplier: {PerfectMultiplier}x"
        );
    }

    private void ResetPerfectCombo()
    {
        PerfectMultiplier = 0;
    }

    private void AddScore(int points)
    {
        if (points <= 0)
            return;

        CurrentScore += points;

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;

            PlayerPrefs.SetInt(
                HighScoreKey,
                HighScore
            );

            PlayerPrefs.Save();
        }

        OnScoreChanged?.Invoke(CurrentScore);

        Debug.Log(
            $"Score +{points} | " +
            $"Score: {CurrentScore} | " +
            $"Best: {HighScore}"
        );
    }

    public void ResetScore()
    {
        CurrentScore = 0;

        DistanceTravelled = 0f;
        distanceAccumulator = 0f;
        CurrentDistanceMilestone = 0;

        ResetPerfectCombo();

        OnScoreChanged?.Invoke(CurrentScore);
    }
}