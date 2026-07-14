using UnityEngine;
using TMPro;

public class RankDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text sText;
    [SerializeField] private TMP_Text aText;
    [SerializeField] private TMP_Text bText;
    [SerializeField] private TMP_Text cText;
    [SerializeField] private TMP_Text dText;
    [SerializeField] private Animator rankAnimator;

    private void Start()
    {
        ResetRankDisplay();  // This will ensure everything is hidden at start.
  
    }

    public void DisplayRank(int score)
    {
        float scorePercentage = (float)score / 75000.0f;
        string rank = CalculateRank(scorePercentage);
        
        ResetRankDisplay();  // This will ensure everything is hidden at start.
  

        // Display the corresponding text
        switch (rank)
        {
            case "S":
                sText.gameObject.SetActive(true);
                rankAnimator.SetTrigger("S");
                break;
            case "A":
                aText.gameObject.SetActive(true);
                rankAnimator.SetTrigger("A");
                break;
            case "B":
                bText.gameObject.SetActive(true);
                rankAnimator.SetTrigger("B");
                break;
            case "C":
                cText.gameObject.SetActive(true);
                rankAnimator.SetTrigger("C");
                break;
            default:
                dText.gameObject.SetActive(true);
                rankAnimator.SetTrigger("D");
                break;
        }
    }

    private string CalculateRank(float scorePercentage)
    {
        if (scorePercentage >= 0.9f)
            return "S";
        if (scorePercentage >= 0.8f)
            return "A";
        if (scorePercentage >= 0.6f)
            return "B";
        if (scorePercentage >= 0.4f)
            return "C";
        return "D";
    }

    public void ResetRankDisplay()
    {
        Debug.Log("Resetting rank display...");
        // Hide all rank texts
        sText.gameObject.SetActive(false);
        aText.gameObject.SetActive(false);
        bText.gameObject.SetActive(false);
        cText.gameObject.SetActive(false);
        dText.gameObject.SetActive(false);

        // Reset the animator's triggers
        rankAnimator.ResetTrigger("S");
        rankAnimator.ResetTrigger("A");
        rankAnimator.ResetTrigger("B");
        rankAnimator.ResetTrigger("C");
        rankAnimator.ResetTrigger("D");
    }



}
