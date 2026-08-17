using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Speed")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float maximumSpeed = 14f;

    [Header("Distance")]
    [SerializeField] private float distanceForMaximumDifficulty = 1000f;

    [Header("Barrier Spacing")]
    [SerializeField] private float startingMinSpacing = 40f;
    [SerializeField] private float startingMaxSpacing = 60f;

    [SerializeField] private float finalMinSpacing = 28f;
    [SerializeField] private float finalMaxSpacing = 42f;

    public float Difficulty01 { get; private set; }

    public float CurrentSpeed { get; private set; }

    public float CurrentMinSpacing { get; private set; }
    public float CurrentMaxSpacing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentSpeed = startingSpeed;
        CurrentMinSpacing = startingMinSpacing;
        CurrentMaxSpacing = startingMaxSpacing;
    }

    private void Update()
    {

        if (GameManager.Instance != null &&
    GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (ScoreManager.Instance == null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        UpdateDifficulty();
    }

    private void UpdateDifficulty()
    {
        float distance =
            ScoreManager.Instance.DistanceTravelled;

        Difficulty01 = Mathf.Clamp01(
            distance / distanceForMaximumDifficulty
        );

        CurrentSpeed = Mathf.Lerp(
            startingSpeed,
            maximumSpeed,
            Difficulty01
        );

        CurrentMinSpacing = Mathf.Lerp(
            startingMinSpacing,
            finalMinSpacing,
            Difficulty01
        );

        CurrentMaxSpacing = Mathf.Lerp(
            startingMaxSpacing,
            finalMaxSpacing,
            Difficulty01
        );

        if (WorldMovement.Instance != null)
        {
            WorldMovement.Instance.SetSpeed(CurrentSpeed);
        }
    }
}