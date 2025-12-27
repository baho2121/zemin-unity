using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Keys
    private const string KEY_COINS = "PlayerCoins";

    // Saving
    public static void SaveGame()
    {
        if (EconomyManager.Instance != null)
        {
            PlayerPrefs.SetInt(KEY_COINS, EconomyManager.Instance.currentCoins);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    // Loading
    public static void LoadGame()
    {
        if (EconomyManager.Instance != null)
        {
            if (PlayerPrefs.HasKey(KEY_COINS))
            {
                int savedCoins = PlayerPrefs.GetInt(KEY_COINS);
                EconomyManager.Instance.currentCoins = savedCoins;
                
                // Force UI update manually since setting variable closely doesn't trigger event
                EconomyManager.Instance.ForceUpdateUI();
            }
        }
        Debug.Log("Game Loaded!");
    }

    // Reset for testing
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Save Data Deleted!");
    }

    // Auto-Save on Quit
    void OnApplicationQuit()
    {
        SaveGame();
    }
    
    // Auto-Save on Pause (Mobile habit, good for PC too)
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }
}
