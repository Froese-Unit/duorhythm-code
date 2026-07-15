using UnityEngine;
using TMPro;
using System.Collections.Generic;  // Required for List
using DG.Tweening;  

public class SAmon : MonoBehaviour
{
    public float shotWindow = 0.17f;  // Length of the shot window
    private List<string> playersShotInCurrentWindow;  // List to track players who shot in the current window
    private float lastShotTime;  // Track the time of the last shot
    public int multiplier = 1;  // Current multiplier
    public TMPro.TextMeshProUGUI multiplierText;

    void Start()
    {
        playersShotInCurrentWindow = new List<string>();  // Initialize the list
        lastShotTime = -shotWindow;  // Initialize to a value that allows shooting immediately
        
        multiplierText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        // Make the multiplierText invisible at the start
        multiplierText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playersShotInCurrentWindow.Count > 0 && Time.time - lastShotTime > shotWindow)
        {
            // Reset multiplier and players shot if the other player doesn't shoot within the window
            multiplier = 1;
            playersShotInCurrentWindow.Clear();
        }
    }

    public void ShotByPlayer(string playerName)
    {
        if (!playersShotInCurrentWindow.Contains(playerName))
        {
            playersShotInCurrentWindow.Add(playerName);
        }

        lastShotTime = Time.time;  // Update the time of the last shot

        if (playersShotInCurrentWindow.Count == 2)  // Both players have shot
        {
            multiplier++;
            ScoreManager.instance.AddScore(100 * multiplier);
            playersShotInCurrentWindow.Clear();  // Reset for the next window
            AnimateMultiplierText();
        }
        else
        {
            ScoreManager.instance.AddScore(0);
        }

        // Handle multiplier text display
        if (multiplier > 1)
        {
            multiplierText.text = multiplier + "x";
            multiplierText.gameObject.SetActive(true); // Ensure the text is active and visible
        }
        else
        {
            multiplierText.gameObject.SetActive(false); // Hide the text when multiplier is 1
        }
    }



        private void AnimateMultiplierText()
    {
        // Reset scale and color to ensure animation works correctly if already scaled or colored
        multiplierText.transform.localScale = Vector3.one;
        multiplierText.color = Color.white;

        // Use DOTween to animate the text scale and color
        Sequence multiplierSequence = DOTween.Sequence();
        multiplierSequence.Append(multiplierText.transform.DOScale(2f, 0.3f)) // Scale up to 2x size
                        .Join(multiplierText.DOColor(Color.yellow, 0.3f))   // Change color to yellow
                        .SetEase(Ease.OutBack)                             // Easing type for scale up
                        .Append(multiplierText.transform.DOScale(1f, 0.3f)) // Scale back to normal size
                        .Join(multiplierText.DOColor(Color.white, 0.3f))    // Change color back to white
                        .SetEase(Ease.InQuad);                             // Easing type for scale down
    }
}
