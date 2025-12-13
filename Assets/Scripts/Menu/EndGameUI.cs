using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Sceny")]
    public string mainMenuSceneName;

    [Header("Slow motion przy œmierci")]
    [Tooltip("Do jakiej wartoœci ma spaœæ timeScale przy œmierci.")]
    public float deathTimeScale = 0.05f;

    [Tooltip("Czas (w sekundach real-time), przez który zwalniamy czas.")]
    public float deathSlowDuration = 1.0f;

    private bool _gameEnded;
    private Coroutine _slowMoRoutine;
    private const float BaseFixedDeltaTime = 0.02f; // domyœlne Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = BaseFixedDeltaTime;

        _gameEnded = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void PlayerDied()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (scoreText != null && GameEconomy.I != null)
        {
            scoreText.text = GameEconomy.I.totalEarnedMoney.ToString();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // p³ynne zwalnianie czasu
        if (_slowMoRoutine != null)
            StopCoroutine(_slowMoRoutine);

        _slowMoRoutine = StartCoroutine(SlowMotionOnDeath());
    }

    private IEnumerator SlowMotionOnDeath()
    {
        float startScale = Time.timeScale;
        float targetScale = Mathf.Clamp(deathTimeScale, 0.001f, 1f);
        float t = 0f;

        while (t < deathSlowDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / deathSlowDuration);

            Time.timeScale = Mathf.Lerp(startScale, targetScale, k);
            Time.fixedDeltaTime = BaseFixedDeltaTime * Time.timeScale;

            yield return null;
        }

        Time.timeScale = targetScale;
        Time.fixedDeltaTime = BaseFixedDeltaTime * Time.timeScale;
        _slowMoRoutine = null;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = BaseFixedDeltaTime;
        _gameEnded = false;

        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void ReturnToMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
            return;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = BaseFixedDeltaTime;
        _gameEnded = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}