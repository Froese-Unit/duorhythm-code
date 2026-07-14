using UnityEngine;
using TMPro;

public class TutorialMimon : MonoBehaviour
{

    public TMPro.TextMeshProUGUI multiplierText;

    void Start()
    {
        multiplierText = GetComponentInChildren<TMPro.TextMeshProUGUI>();

        // Make the multiplierText invisible at the start
        multiplierText.gameObject.SetActive(false);
    }

    public void ShotByPlayer(string playerName)
    {
        UpdateScore(100);  // Simply add 100 points for each shot by a player
    }

    private void UpdateScore(int amount)
    {
        TutorialScoreManager.instance.AddScore(amount);
    }
}
