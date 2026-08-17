using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup transitionFade;
    [SerializeField] private GameObject instructionPanel;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float firstTimeDelay = 0.5f;
    [SerializeField] private float normalStartDelay = 1f;

    private const string InstructionShownKey =
        "GameplayInstructionShown";

    private bool waitingForInput;
    private bool gameStarted;

    private void Start()
    {
        // Make absolutely sure the instruction starts hidden.
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        StartCoroutine(BeginGameplayFlow());
    }

    private IEnumerator BeginGameplayFlow()
    {
        // =================================================
        // MAKE SURE GAMEPLAY IS STOPPED
        // =================================================

        if (WorldMovement.Instance != null)
        {
            WorldMovement.Instance.StopMoving();
        }

        // =================================================
        // START BLACK
        // =================================================

        if (transitionFade != null)
        {
            transitionFade.alpha = 1f;
            transitionFade.blocksRaycasts = true;
        }

        // =================================================
        // FADE INTO GAMEPLAY
        // =================================================

        yield return Fade(
            1f,
            0f,
            fadeInDuration
        );

        if (transitionFade != null)
        {
            transitionFade.blocksRaycasts = false;
        }

        // =================================================
        // CHECK FIRST-TIME INSTRUCTION
        // =================================================

        bool instructionShown =
            PlayerPrefs.GetInt(
                InstructionShownKey,
                0
            ) == 1;

        // =================================================
        // FIRST TIME PLAYER
        // =================================================

        if (!instructionShown)
        {
            yield return StartFirstTimeFlow();
        }
        else
        {
            // =================================================
            // RETURNING PLAYER
            // =================================================

            // Instruction remains hidden.
            // Give the player a short moment to get ready.
            yield return new WaitForSeconds(
                normalStartDelay
            );

            StartGameplay();
        }
    }

    // =================================================
    // FIRST TIME PLAYER
    // =================================================

    private IEnumerator StartFirstTimeFlow()
    {
        // Give the player a little breathing room
        // after the environment becomes visible.
        yield return new WaitForSeconds(
            firstTimeDelay
        );

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }

        waitingForInput = true;

        Debug.Log(
            "FIRST LAUNCH: Instruction displayed. Waiting for player tap."
        );
    }

    // =================================================
    // INPUT
    // =================================================

    private void Update()
    {
        if (!waitingForInput)
            return;

        if (gameStarted)
            return;

        if (WasStartPressed())
        {
            StartFromInstruction();
        }
    }

    private bool WasStartPressed()
    {
        // Unity Editor / mouse testing
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        // Android / mobile touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    // =================================================
    // START FROM INSTRUCTION
    // =================================================

    private void StartFromInstruction()
    {
        waitingForInput = false;

        // Remember that the first-time instruction
        // has now been completed.
        PlayerPrefs.SetInt(
            InstructionShownKey,
            1
        );

        PlayerPrefs.Save();

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        StartGameplay();
    }

    // =================================================
    // START GAMEPLAY
    // =================================================

    private void StartGameplay()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        // Safety check.
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        // Resume GameManager.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        // Start world movement.
        if (WorldMovement.Instance != null)
        {
            WorldMovement.Instance.StartMoving();
        }

        // Starting gameplay audio.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStart();
        }

        Debug.Log("GAMEPLAY STARTED");
    }

    // =================================================
    // FADE
    // =================================================

    private IEnumerator Fade(
        float start,
        float end,
        float duration)
    {
        if (transitionFade == null)
            yield break;

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

            transitionFade.alpha =
                Mathf.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }

        transitionFade.alpha = end;
    }
}