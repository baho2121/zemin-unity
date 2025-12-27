using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    // Data
    public int currentCoins = 0;

    // Events
    public event Action<int> OnCoinsChanged; // Listeners can subscribe to this

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Load data when game starts
        SaveManager.LoadGame();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        
        // Notify UI or other systems
        OnCoinsChanged?.Invoke(currentCoins);

        // Optional: Save every time we get money? (Safe but can be slow if too frequent)
        // For performance, better to rely on Auto-Save or Interval Save.
        // SaveManager.SaveGame(); 
    }

    // Helper to update UI after loading
    public void ForceUpdateUI()
    {
        OnCoinsChanged?.Invoke(currentCoins);
    }
}
