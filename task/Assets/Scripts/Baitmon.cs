using UnityEngine;
using TMPro;
public class Baitmon : MonoBehaviour
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
        UpdateScore(-500);  // Deduct 500 points for shooting a Baitmon
    }

    private void UpdateScore(int amount)
    {
        ScoreManager.instance.AddScore(amount);
    }
}
