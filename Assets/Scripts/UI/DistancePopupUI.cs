using System.Collections;
using TMPro;
using UnityEngine;

public class DistancePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private float displayTime = 1f;

    private Coroutine currentRoutine;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceMilestone +=
                ShowDistance;
        }

        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceMilestone -=
                ShowDistance;
        }
    }

    private void ShowDistance(int distance)
    {
        if (distanceText == null)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                ShowRoutine(distance)
            );
    }

    private IEnumerator ShowRoutine(int distance)
    {
        distanceText.gameObject.SetActive(true);

        distanceText.text =
            $"{distance} M";

        yield return new WaitForSeconds(displayTime);

        distanceText.gameObject.SetActive(false);
    }
}