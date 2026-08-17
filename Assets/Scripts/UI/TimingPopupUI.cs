using System.Collections;
using TMPro;
using UnityEngine;

public class TimingPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float displayTime = 0.7f;

    private Coroutine currentRoutine;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnTimingResult += ShowTiming;
        }

        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnTimingResult -= ShowTiming;
        }
    }

    private void ShowTiming(
        PulseTiming timing,
        int points,
        int multiplier)
    {
        if (popupText == null)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(ShowRoutine(
                timing,
                points,
                multiplier
            ));
    }

    private IEnumerator ShowRoutine(
        PulseTiming timing,
        int points,
        int multiplier)
    {
        popupText.gameObject.SetActive(true);

        switch (timing)
        {
            case PulseTiming.Perfect:

                popupText.text =
                    $"PERFECT!\n+{points}";

                break;

            case PulseTiming.Good:

                popupText.text =
                    $"GOOD\n+{points}";

                break;

            case PulseTiming.Early:

                popupText.text =
                    "EARLY";

                break;

            default:

                popupText.gameObject.SetActive(false);

                yield break;
        }

        yield return new WaitForSeconds(displayTime);

        popupText.gameObject.SetActive(false);
    }
}