using UnityEngine;

// Quản lý máu và trạng thái bị phá hủy của Lõi Trung Tâm.
public class EnergyCore : MonoBehaviour
{
    [Header("Chỉ số Lõi Trung Tâm")]
    public float maxHP = 1000f;
    public float currentHP;

    // Tỉ lệ máu từ 0 đến 1, được GameHUD dùng để hiển thị thanh máu.
    public float HealthPercent => maxHP <= 0f ? 0f : currentHP / maxHP;

    // Khởi tạo Lõi với đầy máu khi màn chơi bắt đầu.
    void Start()
    {
        currentHP = maxHP;
    }

    // Được quái gọi liên tục khi chúng chạm vào Lõi.
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);

        Debug.Log($"Lõi Trung Tâm còn {currentHP}/{maxHP} HP");

        if (currentHP <= 0f)
            GameOver();
    }

    // Tạm dừng toàn bộ trò chơi khi Lõi hết máu.
    void GameOver()
    {
        Debug.Log("=== GAME OVER: Lõi Trung Tâm đã bị phá hủy! ===");
        // Sau này có thể gọi màn hình Game Over tại đây.
        Time.timeScale = 0f;
    }
}
