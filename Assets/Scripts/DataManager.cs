using UnityEngine;

public static class DataManager
{
    //This key is used to save data to the computer's memory (PlayerPres).
    private const string MONEY_KEY = "TotalMoney";
    private const string DAY_KEY = "CurrentDay";

    //The Money property (Can be read from anywhere, only modified within this class or via a function)
    public static int TotalMoney
    {
        get => PlayerPrefs.GetInt(MONEY_KEY, 0); //The default value is 0 if there is no data.
        private set => PlayerPrefs.SetInt(MONEY_KEY, value);
    }
 

    public static int CurrentDay
    {
        get => PlayerPrefs.GetInt(DAY_KEY, 1); //The default first day is 1.
        private set => PlayerPrefs.SetInt(DAY_KEY, value);
    }

    // Function to add money
    public static void AddMoney(int amount)
    {
        TotalMoney += amount;
        PlayerPrefs.Save(); // Save immediately to drive
    }

    // The function increments the number of days by 1.
    public static void AdvanceDay()
    {
        CurrentDay += 1;
        PlayerPrefs.Save();
    }

    // Use this function if you want to reset the game from the beginning (Optional)
    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}