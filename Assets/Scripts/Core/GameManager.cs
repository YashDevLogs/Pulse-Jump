using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Paused;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerPulse playerPulse;
    [SerializeField] private PulseEnergy pulseEnergy;
    [SerializeField] private Shockwave shockwave;

    [Header("Effects")]
    [SerializeField] private ParticleSystem gameOverEffect;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Level";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================================================
    // START
    // =========================================================

    public void StartGame()
    {
        CurrentState = GameState.Playing;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (WorldMovement.Instance != null)
            WorldMovement.Instance.StartMoving();

        Debug.Log("GAMEPLAY STARTED");
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Paused;

        StopGameplay();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("GAME PAUSED");
    }

    // =========================================================
    // RESUME
    // =========================================================

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;

        CurrentState = GameState.Playing;

        if (pulseEnergy != null)
            pulseEnergy.enabled = true;

        if (playerPulse != null)
            playerPulse.enabled = true;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (WorldMovement.Instance != null)
            WorldMovement.Instance.StartMoving();

        Debug.Log("GAME RESUMED");
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver)
            return;

        CurrentState = GameState.GameOver;

        StopGameplay();

        // Stop active shockwave
        if (shockwave != null)
            shockwave.Cancel();

        // Stop pulse animation
        if (playerPulse != null)
            playerPulse.CancelPulse();

        // Play death effect
        if (gameOverEffect != null)
        {
            gameOverEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            gameOverEffect.Play();
        }

        // Show Game Over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

             if (AudioManager.Instance != null)
        AudioManager.Instance.PlayGameOver();


        Debug.Log("GAME OVER");
    }

    // =========================================================
    // STOP GAMEPLAY SYSTEMS
    // =========================================================

    private void StopGameplay()
    {
        if (WorldMovement.Instance != null)
            WorldMovement.Instance.StopMoving();

        if (pulseEnergy != null)
            pulseEnergy.enabled = false;

        if (playerPulse != null)
            playerPulse.enabled = false;
    }

    // =========================================================
    // RETRY
    // =========================================================

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuScene);
    }
}