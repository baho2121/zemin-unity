using UnityEngine;
using UnityEngine.UI; // For standard Text
// using TMPro; // Uncomment if utilizing TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Text coinText; // Assign a legacy Text or TMP object here
    // public TMPro.TextMeshProUGUI coinTmpText; // Use this if you prefer TMP

    void Start()
    {
        // Subscribe to economy updates
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged += UpdateCoinUI;
            // Initialize
            UpdateCoinUI(EconomyManager.Instance.currentCoins);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnCoinsChanged -= UpdateCoinUI;
    }

    void UpdateCoinUI(int amount)
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + amount.ToString("N0"); // Formats as 1,000
        }
        else
        {
            // Debug.LogWarning("UIManager: Coin Text not assigned!");
        }
    }
}
