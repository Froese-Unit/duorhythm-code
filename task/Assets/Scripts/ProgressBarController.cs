using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressBarController : MonoBehaviour
{
    public Slider progressBar; // Reference to the slider
    public int totalBlocks = 10; // Total number of blocks in the game
    private int currentBlock = 0; // Current block, used for tracking
    public Image fillImage; // Reference to the Image component of the progress bar fill
    public Image backgroundImage; // Reference to the background Image of the progress bar

    private Color originalFillColor; // Store the original fill color
    private Color originalBackgroundColor; // Store the original background color

    void Start()
    {
        if (progressBar == null)
        {
            progressBar = GetComponentInChildren<Slider>();
            if (progressBar == null)
            {
                Debug.LogError("ProgressBar is not assigned and could not be found as a child object.");
                return;
            }
        }

        if (fillImage == null)
        {
            fillImage = progressBar.fillRect.GetComponent<Image>(); // Assign the fill image
            if (fillImage == null)
            {
                Debug.LogError("Fill image of the progress bar is not assigned.");
                return;
            }
        }

        if (backgroundImage == null)
        {
            backgroundImage = progressBar.GetComponentInChildren<Image>(); // Assign the background image
            if (backgroundImage == null)
            {
                Debug.LogError("Background image of the progress bar is not assigned.");
                return;
            }
        }


        // Initialize progress bar
        progressBar.minValue = 0;
        progressBar.maxValue = totalBlocks;
        progressBar.value = 0; // Initialize with starting block
    }

    // Called when the block number changes
    public void UpdateProgressBar(int newBlock)
    {
        currentBlock = newBlock; // Update the current block
        progressBar.DOValue(currentBlock, 2.0f).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                StartBlinking();
            }); // Call MakeBrighter and StartBlinking after animation completes

        Debug.Log($"Progress bar updated to block: {currentBlock}");
    }



    // Method to start blinking effect
    private void StartBlinking()
    {
        Debug.Log("StartBlinking method called.");

        // Blinking effect by fading the alpha value of the fill image
        fillImage.DOColor(new Color(fillImage.color.r, fillImage.color.g, fillImage.color.b, 0f), 0.5f)
            .SetLoops(6, LoopType.Yoyo) // Blink 3 times (6 loops with Yoyo)
            .SetEase(Ease.InOutSine)
            .OnKill(() => Debug.Log("Blinking tween killed."));
    }

    // Get current block number
    public int GetCurrentBlock()
    {
        return currentBlock;
    }
}
