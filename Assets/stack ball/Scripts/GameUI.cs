using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public static bool autoStartGame = false;

    [Header("Panels")]
    public GameObject startMenuPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("In-Game UI & Controls")]
    public GameObject inGameUI;

    [Header("Level Progress Bar")]
    public Slider levelProgressBar;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;

    [Header("Game Over & Score UI")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverBestText;

    [Header("Invincible Meter")]
    public Slider invincibleSlider;
    public Image invincibleFillImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClip;

    private Ball ball;
    private Rotator rotator;
    private int initialRemainingStacks = 0;

    void Awake()
    {
        instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        ball = Object.FindFirstObjectByType<Ball>();
        rotator = Object.FindFirstObjectByType<Rotator>();

        UpdateLevelTextDisplay();

        if (levelProgressBar != null)
        {
            levelProgressBar.minValue = 0f;
            levelProgressBar.maxValue = 1f;
            levelProgressBar.value = 0f;
        }

        if (autoStartGame)
        {
            if (startMenuPanel != null) startMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);
            if (inGameUI != null) inGameUI.SetActive(true);

            if (ball != null)
            {
                ball.ballState = BallState.Playing;
            }
        }
        else
        {
            if (startMenuPanel != null) startMenuPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);
            if (inGameUI != null) inGameUI.SetActive(false);
        }

        Invoke(nameof(RecordInitialStacks), 0.05f);
    }

    public void UpdateLevelTextDisplay()
    {
        int currentLvl = PlayerPrefs.GetInt("Level", 1);
        if (currentLevelText != null) currentLevelText.text = currentLvl.ToString();
        if (nextLevelText != null) nextLevelText.text = (currentLvl + 1).ToString();
    }

    void RecordInitialStacks()
    {
        if (rotator != null)
        {
            initialRemainingStacks = rotator.transform.childCount;
        }
    }

    void Update()
    {
        // 1. Invincible Meter Bar
        if (ball != null && invincibleSlider != null)
        {
            invincibleSlider.value = ball.currentTime;
            if (invincibleFillImage != null)
            {
                invincibleFillImage.color = ball.invincible ? Color.red : new Color(1f, 0.6f, 0f);
            }
        }

        // 2. Level Progress Bar Calculation
        if (rotator != null && levelProgressBar != null && initialRemainingStacks > 0)
        {
            int currentRemaining = rotator.transform.childCount;
            int broken = initialRemainingStacks - currentRemaining;
            levelProgressBar.value = Mathf.Clamp01((float)broken / initialRemainingStacks);
        }
    }

    public void StartGame()
    {
        PlayButtonSound();

        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(true);

        if (ball != null)
        {
            ball.ballState = BallState.Playing;
        }
    }

    public void GoToHome()
    {
        PlayButtonSound();
        autoStartGame = false;
        Invoke(nameof(ReloadScene), 0.15f);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (ScoreManager.instance != null)
            {
                if (gameOverScoreText != null)
                    gameOverScoreText.text = "SCORE: " + ScoreManager.instance.score;

                if (gameOverBestText != null)
                    gameOverBestText.text = "BEST: " + ScoreManager.instance.bestScore;
            }
        }
    }

    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        PlayButtonSound();
        autoStartGame = true;

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
        }

        Invoke(nameof(ReloadScene), 0.15f);
    }

    public void NextLevel()
    {
        PlayButtonSound();
        autoStartGame = true;

        int currentLvl = PlayerPrefs.GetInt("Level", 1);
        PlayerPrefs.SetInt("Level", currentLvl + 1);
        PlayerPrefs.Save();

        Invoke(nameof(ReloadScene), 0.15f);
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonClip != null)
        {
            audioSource.PlayOneShot(buttonClip);
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}