using UnityEngine;

public static class DataManager
{
    // Keys used to save data to the computer's memory (PlayerPrefs).
    private const string MONEY_KEY = "TotalMoney";
    private const string DAY_KEY = "CurrentDay";

    // Public properties to READ data from anywhere
    public static int TotalMoney => PlayerPrefs.GetInt(MONEY_KEY, 0);
    public static int CurrentDay => PlayerPrefs.GetInt(DAY_KEY, 1);

    // Explicit function to add money and force write to PlayerPrefs
    public static void AddMoney(int amount)
    {
        int currentMoney = PlayerPrefs.GetInt(MONEY_KEY, 0);
        int newMoney = currentMoney + amount;

        PlayerPrefs.SetInt(MONEY_KEY, newMoney);
        PlayerPrefs.Save(); // Force save immediately to disk

        Debug.Log($"[DataManager] Added ${amount}. New Total: ${newMoney}");
    }

    // Explicit function to advance day and force write to PlayerPrefs
    public static void AdvanceDay()
    {
        int currentDay = PlayerPrefs.GetInt(DAY_KEY, 1);
        int nextDay = currentDay + 1;

        PlayerPrefs.SetInt(DAY_KEY, nextDay);
        PlayerPrefs.Save(); // Force save immediately to disk

        Debug.Log($"[DataManager] Day advanced to: {nextDay}");
    }

    // Reset all game data (Optional)
    public static void ResetData()
    {
        PlayerPrefs.DeleteKey(MONEY_KEY);
        PlayerPrefs.DeleteKey(DAY_KEY);
        PlayerPrefs.Save();
        Debug.Log("[DataManager] All data has been reset.");
    }
}