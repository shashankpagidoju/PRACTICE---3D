using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Score Data")]
    public int score = 0;
    public int bestScore = 0;

    [Header("In-Game Score Text")]
    public TextMeshProUGUI inGameScoreText;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (inGameScoreText != null)
        {
            inGameScoreText.text = score.ToString();
        }
    }
}