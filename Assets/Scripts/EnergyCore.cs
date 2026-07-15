using UnityEngine;

public class EnergyCore : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHP = 1000f;
    public float currentHP;

    [Header("Idle Animation")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public float idleAnimationFps = 5f;

    public float HealthPercent => maxHP <= 0f ? 0f : currentHP / maxHP;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer ??= GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer == null || idleSprites == null || idleSprites.Length == 0)
            return;

        int frame = Mathf.FloorToInt(Time.time * Mathf.Max(1f, idleAnimationFps)) % idleSprites.Length;
        if (idleSprites[frame] != null)
            spriteRenderer.sprite = idleSprites[frame];
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
