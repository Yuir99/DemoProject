using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 100f;
    [SerializeField] private float currentHP;

    [Header("Experience")]
    public float xpToNextLevel = 100f;
    [SerializeField] private float currentXP = 0f;
    [SerializeField] private int level = 1;

    public float CurrentHP => currentHP;
    public float CurrentXP => currentXP;
    public int Level => level;
    public float HealthPercent => maxHP <= 0f ? 0f : currentHP / maxHP;
    public float XPPercent => xpToNextLevel <= 0f ? 0f : currentXP / xpToNextLevel;
    public event Action<int> LeveledUp;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0f);
        Debug.Log($"Player HP {currentHP}/{maxHP}");

        if (currentHP <= 0f)
            Die();
    }

    public void GainXP(float amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel && xpToNextLevel > 0f)
            LevelUp();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel *= 1.2f;
        Debug.Log($"Level up: {level}");
        LeveledUp?.Invoke(level);
    }

    public void UpgradeMaxHealth(float amount)
    {
        maxHP += amount;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    void Die()
    {
        Debug.Log("Game over: player died.");
        Time.timeScale = 0f;
    }
}
