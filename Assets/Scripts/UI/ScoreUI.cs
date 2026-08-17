using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Combo")]
    [SerializeField] private TMP_Text comboText;

    private void Start()
    {
        if (ScoreManager.Instance == null)
            return;

        ScoreManager.Instance.OnScoreChanged += UpdateScore;
        ScoreManager.Instance.OnTimingResult += UpdateCombo;

        UpdateScore(
            ScoreManager.Instance.CurrentScore
        );

        UpdateCombo(
            PulseTiming.None,
            0,
            ScoreManager.Instance.PerfectMultiplier
        );
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance == null)
            return;

        ScoreManager.Instance.OnScoreChanged -= UpdateScore;
        ScoreManager.Instance.OnTimingResult -= UpdateCombo;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("N0");
        }

        if (highScoreText != null)
        {
            highScoreText.text =
                $"BEST {ScoreManager.Instance.HighScore:N0}";
        }
    }

    private void UpdateCombo(
        PulseTiming timing,
        int points,
        int multiplier)
    {
        if (comboText == null)
            return;

        if (multiplier <= 0)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.gameObject.SetActive(true);

        comboText.text = $"{multiplier}x";
    }
}