using UnityEngine;
using UnityEngine.UI;

public class FollowProgressBar : MonoBehaviour
{
    public Slider progressBar;          // Reference to the Slider component (progress bar)
    public RectTransform runningGreen;  // Reference to the RectTransform of the RunningGreen object
    public float yOffset = -30f;        // Offset to position runningGreen below the progress bar
    public float xOffset = -80.0f;
    void Update()
    {
        if (progressBar != null && runningGreen != null)
        {
            // Calculate the fill amount of the progress bar (value between 0 and 1)
            float fillAmount = progressBar.value;

            Vector2 newPosition = new Vector2(xOffset  + (20.0f * fillAmount) - ( ((fillAmount-1)*5.0f)), yOffset);

            // Update runningGreen's local position relative to the fill
            runningGreen.localPosition = newPosition;
        }
    }
}
