using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Level";

    [Header("Main Menu UI")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform highScore;
    [SerializeField] private TMP_Text tapToStart;

    [Header("Ball")]
    [SerializeField] private MainMenuPlayerAnimation menuBall;

    [Header("Transition")]
    [SerializeField] private CanvasGroup transitionFade;
    [SerializeField] private float transitionDuration = 0.35f;

    [Header("Logo Animation")]
    [SerializeField] private float logoAnimationDuration = 0.65f;

    [Header("High Score Animation")]
    [SerializeField] private float highScoreAnimationDuration = 1f;
    [SerializeField] private float highScoreDelay = 0.15f;
    [SerializeField] private float highScoreOvershoot = 1.08f;

    [Header("Tap To Start")]
    [SerializeField] private float tapToStartDelay = 0.15f;
    [SerializeField] private float blinkDuration = 1.2f;
    [SerializeField] private float minimumTapAlpha = 0.35f;

    private Vector2 logoFinalPosition;
    private Vector2 highScoreFinalPosition;

    private Vector2 logoStartPosition;
    private Vector2 highScoreStartPosition;

    private bool introFinished;
    private bool isStarting;

    private Coroutine blinkCoroutine;


    private void Awake()
    {
        // -----------------------------------------
        // SAVE FINAL UI POSITIONS
        // -----------------------------------------

        if (logo != null)
        {
            logoFinalPosition =
                logo.anchoredPosition;

            logoStartPosition =
                logoFinalPosition +
                Vector2.up * 1000f;
        }

        if (highScore != null)
        {
            highScoreFinalPosition =
                highScore.anchoredPosition;

            highScoreStartPosition =
                highScoreFinalPosition -
                Vector2.up * 500f;
        }


        // -----------------------------------------
        // INITIAL UI STATE
        // -----------------------------------------

        if (logo != null)
        {
            logo.anchoredPosition =
                logoStartPosition;

            logo.localScale =
                Vector3.one * 0.85f;
        }

        if (highScore != null)
        {
            highScore.anchoredPosition =
                highScoreStartPosition;

            highScore.localScale =
                Vector3.one * 0.85f;
        }

        if (tapToStart != null)
        {
            tapToStart.gameObject.SetActive(false);
        }

        // -----------------------------------------
        // FADE
        // -----------------------------------------

        if (transitionFade != null)
        {
            transitionFade.alpha = 0f;
            transitionFade.blocksRaycasts = false;
            transitionFade.interactable = false;
        }
    }


    private void Start()
    {
        UpdateHighScore();

        StartCoroutine(IntroSequence());
    }


    // =========================================================
    // INTRO SEQUENCE
    // =========================================================

    private IEnumerator IntroSequence()
    {
        Debug.Log("Main Menu intro started.");


        // -----------------------------------------
        // LOGO
        // -----------------------------------------

        yield return AnimateFromTop(
            logo,
            logoStartPosition,
            logoFinalPosition,
            logoAnimationDuration
        );


        // -----------------------------------------
        // HIGH SCORE
        // -----------------------------------------

        yield return new WaitForSeconds(
            highScoreDelay
        );

        yield return AnimateFromBottom(
            highScore,
            highScoreStartPosition,
            highScoreFinalPosition,
            highScoreAnimationDuration
        );


        // -----------------------------------------
        // BALL
        // -----------------------------------------

        // The ball animation already starts
        // automatically from MainMenuPlayerAnimation.

        // Give it a moment to enter the scene.
        yield return new WaitForSeconds(0.2f);


        // -----------------------------------------
        // TAP TO START
        // -----------------------------------------

        yield return new WaitForSeconds(
            tapToStartDelay
        );

        EnableTapToStart();


        introFinished = true;

        Debug.Log(
            "Main Menu intro finished. " +
            "Waiting for input."
        );
    }


    // =========================================================
    // LOGO ANIMATION
    // =========================================================

    private IEnumerator AnimateFromTop(
    RectTransform target,
    Vector2 startPosition,
    Vector2 finalPosition,
    float duration)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;

        // Start smaller.
        target.localScale = Vector3.one * 0.72f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            // -----------------------------------------
            // POSITION
            // -----------------------------------------

            // Smooth deceleration as the logo falls.
            float positionT =
                Mathf.SmoothStep(0f, 1f, t);

            // Very subtle positional overshoot.
            float overshoot =
                Mathf.Sin(t * Mathf.PI) * 0.035f;

            positionT += overshoot;

            target.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    finalPosition,
                    positionT
                );

            // -----------------------------------------
            // SCALE
            // -----------------------------------------

            // First grow naturally while entering.
            float scaleT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            float scale =
                Mathf.Lerp(
                    0.72f,
                    1f,
                    scaleT
                );

            target.localScale =
                Vector3.one * scale;

            yield return null;
        }

        // -----------------------------------------
        // SOFT LANDING / MICRO BOUNCE
        // -----------------------------------------

        float bounceDuration = 0.28f;
        elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / bounceDuration
                );

            // Damped sine.
            float bounce =
                Mathf.Sin(
                    t * Mathf.PI * 2f
                ) *
                (1f - t) *
                0.035f;

            target.localScale =
                Vector3.one *
                (1f + bounce);

            yield return null;
        }

        // -----------------------------------------
        // FINAL STATE
        // -----------------------------------------

        target.anchoredPosition =
            finalPosition;

        target.localScale =
            Vector3.one;
    }


    // =========================================================
    // HIGH SCORE ANIMATION
    // =========================================================

    private IEnumerator AnimateFromBottom(
        RectTransform target,
        Vector2 startPosition,
        Vector2 finalPosition,
        float duration)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float eased =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            target.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    finalPosition,
                    eased
                );

            float scale =
                Mathf.Lerp(
                    0.85f,
                    highScoreOvershoot,
                    eased
                );

            target.localScale =
                Vector3.one * scale;

            yield return null;
        }

        target.anchoredPosition =
            finalPosition;

        target.localScale =
            Vector3.one;
    }


    // =========================================================
    // TAP TO START
    // =========================================================

    private void EnableTapToStart()
    {
        if (tapToStart == null)
            return;

        tapToStart.gameObject.SetActive(true);

        Color color =
            tapToStart.color;

        color.a = 1f;

        tapToStart.color = color;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine =
            StartCoroutine(
                BlinkTapToStart()
            );
    }


    private IEnumerator BlinkTapToStart()
    {
        while (!isStarting)
        {
            float elapsed = 0f;

            while (
                elapsed < blinkDuration &&
                !isStarting
            )
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / blinkDuration
                    );

                float alpha =
                    Mathf.Lerp(
                        1f,
                        minimumTapAlpha,
                        t
                    );

                Color color =
                    tapToStart.color;

                color.a = alpha;

                tapToStart.color = color;

                yield return null;
            }


            elapsed = 0f;

            while (
                elapsed < blinkDuration &&
                !isStarting
            )
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / blinkDuration
                    );

                float alpha =
                    Mathf.Lerp(
                        minimumTapAlpha,
                        1f,
                        t
                    );

                Color color =
                    tapToStart.color;

                color.a = alpha;

                tapToStart.color = color;

                yield return null;
            }
        }
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void Update()
    {
        if (!introFinished)
            return;

        if (isStarting)
            return;

        if (WasStartPressed())
        {
            StartGame();
        }
    }


    private bool WasStartPressed()
    {
        // Mouse
        if (
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            return true;
        }


        // Touch
        if (
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch
                .press.wasPressedThisFrame
        )
        {
            return true;
        }

        return false;
    }


    // =========================================================
    // START GAME
    // =========================================================

    private void StartGame()
    {
        if (isStarting)
            return;

        isStarting = true;

        Debug.Log(
            "Main Menu: Starting game."
        );

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        StartCoroutine(
            StartGameRoutine()
        );
    }


    private IEnumerator StartGameRoutine()
    {
        // -----------------------------------------
        // FADE TO BLACK
        // -----------------------------------------

        if (transitionFade != null)
        {
            transitionFade.blocksRaycasts = true;
            transitionFade.interactable = true;

            yield return FadeCanvas(
                0f,
                1f,
                transitionDuration
            );
        }


        // -----------------------------------------
        // LOAD GAMEPLAY
        // -----------------------------------------

        SceneManager.LoadScene(
            gameplaySceneName
        );
    }


    private IEnumerator FadeCanvas(
        float start,
        float end,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            if (transitionFade != null)
            {
                transitionFade.alpha =
                    Mathf.Lerp(
                        start,
                        end,
                        t
                    );
            }

            yield return null;
        }

        if (transitionFade != null)
        {
            transitionFade.alpha = end;
        }
    }


    // =========================================================
    // HIGH SCORE
    // =========================================================

    private void UpdateHighScore()
    {
        if (highScore == null)
            return;

        TMP_Text text =
            highScore.GetComponentInChildren<TMP_Text>();

        if (text == null)
            return;

        int score =
            PlayerPrefs.GetInt(
                "HighScore",
                0
            );

        text.text =
            $"HIGH SCORE\n{score}";
    }
}