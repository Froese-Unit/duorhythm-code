using UnityEngine;
using TMPro;
using DG.Tweening;  

public class TutorialTTmon : MonoBehaviour
{
    public float shotWindowStart = 0.35f;  // Start of the shot window
    public float shotWindowEnd = 0.65f;  // End of the shot window
    public static string lastPlayerShot = "";  // Track the last player who shot
    public static float lastShotTime = -10f;  // Track the last shot time
    public int multiplier = 1;  // Current multiplier
    public TMPro.TextMeshProUGUI multiplierText;

    void Start()
    {
        multiplierText = GetComponentInChildren<TMPro.TextMeshProUGUI>();

        // Make the multiplierText invisible at the start
        multiplierText.gameObject.SetActive(false);
    }



    void Update()
    {
        if (lastPlayerShot != "" && Time.time - lastShotTime > shotWindowEnd)
        {
            // Reset multiplier and last player shot if the other player doesn't shoot within the window
            multiplier = 1;
            lastPlayerShot = "";
        }
    }

    public void ShotByPlayer(string playerName)
    {
        if (lastPlayerShot == "")
        {
            // This is the first shot
            multiplier = 1;
            TutorialScoreManager.instance.AddScore(0);
        }
        else if (playerName != lastPlayerShot && Time.time - lastShotTime >= shotWindowStart && Time.time - lastShotTime <= shotWindowEnd)
        {
            // Successful consecutive shot by the other player within the window
            multiplier++;
            TutorialScoreManager.instance.AddScore(100 * multiplier);

            AnimateMultiplierText();
        }
        else
        {
            // Reset multiplier if shot outside the window or if the same player shoots consecutively
            multiplier = 1;
            TutorialScoreManager.instance.AddScore(0);
        }

        lastPlayerShot = playerName;
        lastShotTime = Time.time;

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
