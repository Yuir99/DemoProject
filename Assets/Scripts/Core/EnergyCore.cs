using UnityEngine;
using UnityEngine.UI;

public class EnergyCore : MonoBehaviour
{
    [Header("Chỉ số Lò Năng Lượng")]
    public float maxHP = 1000f;
    public float currentHP;
    public float HealthPercent => maxHP <= 0f ? 0f : currentHP / maxHP;

    // Gọi hàm này khi muốn trừ máu
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f); // Không xuống dưới 0

        Debug.Log($"Lò năng lượng còn {currentHP}/{maxHP} HP");

        if (currentHP <= 0f)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("=== GAME OVER: Lò Năng Lượng bị phá hủy! ===");
        // TODO: Hiện màn hình Game Over
        Time.timeScale = 0f; // Dừng game
    }

    void Start()
    {
        currentHP = maxHP;
    }
}
