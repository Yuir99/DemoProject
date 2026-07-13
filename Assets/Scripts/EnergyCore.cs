using UnityEngine;

public class EnergyCore : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHP = 1000f;
    public float currentHP;

    public float HealthPercent => maxHP <= 0f ? 0f : currentHP / maxHP;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.GameHasEnded)
            return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);

        if (currentHP <= 0f)
            GameOver();
    }

    void GameOver()
    {
        Debug.Log("=== GAME OVER: Energy Core destroyed. ===");

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.LoseGame();
        else
            Time.timeScale = 0f;
    }
}
