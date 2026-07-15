using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // Singleton instance

    public int score = 0;
    public TextMeshProUGUI scoreText;
    public GameObject scoreUI; // Reference to the GameObject holding the score UI
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points)
    {
        score += points;

        // Ensure score never goes below zero
        if (score < 0)
        {
            score = 0;
        }

        scoreText.text = "Fuel: " + score;
    }

    public void HideScoreUI()
    {
        scoreUI.SetActive(false);
    }

    public void ResetScore()
    {
        score = 0;
        scoreText.text = "Fuel: " + score;
        scoreUI.SetActive(true);
    }
}
