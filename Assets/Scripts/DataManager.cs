using UnityEngine;

public static class DataManager
{
    // Khóa dùng để lưu vào bộ nhớ máy (PlayerPrefs)
    private const string MONEY_KEY = "TotalMoney";
    private const string DAY_KEY = "CurrentDay";

    // Thuộc tính Tiền (Có thể đọc từ bất cứ đâu, chỉ sửa trong class này hoặc qua hàm)
    public static int TotalMoney
    {
        get => PlayerPrefs.GetInt(MONEY_KEY, 0); // Mặc định là 0 nếu chưa có dữ liệu
        private set => PlayerPrefs.SetInt(MONEY_KEY, value);
    }

    // Thuộc tính Ngày
    public static int CurrentDay
    {
        get => PlayerPrefs.GetInt(DAY_KEY, 1); // Mặc định ngày đầu tiên là 1
        private set => PlayerPrefs.SetInt(DAY_KEY, value);
    }

    // Hàm cộng thêm tiền
    public static void AddMoney(int amount)
    {
        TotalMoney += amount;
        PlayerPrefs.Save(); // Lưu ngay lập tức vào ổ đĩa
    }

    // Hàm tăng số ngày lên 1
    public static void AdvanceDay()
    {
        CurrentDay += 1;
        PlayerPrefs.Save();
    }

    // Hàm dùng nếu bạn muốn reset game từ đầu (Tùy chọn)
    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}