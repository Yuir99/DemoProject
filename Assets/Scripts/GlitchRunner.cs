using UnityEngine;

public class GlitchRunner : EnemyBase
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float flickerTimer;

    protected override void Start()
    {
        base.Start();

        maxHP = 25f;
        currentHP = maxHP;
        moveSpeed = 3.6f;
        damage = 5f;
        soulDropType = SoulType.Speed;
        soulDropCount = 1;
        xpReward = 3f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = new Color(0.5f, 1f, 0.5f, 1f);
            spriteRenderer.enabled = true;
            spriteRenderer.color = baseColor;
        }

        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        if (hitbox != null)
            hitbox.radius = 0.38f;
    }

    protected override void Update()
    {
        base.Update();
        UpdateVisualFlicker();
    }

    void UpdateVisualFlicker()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.enabled = true;

        if (flickerTimer > 0f)
        {
            flickerTimer -= Time.deltaTime;
            float pulse = Mathf.PingPong(Time.time * 28f, 1f);
            spriteRenderer.color = Color.Lerp(baseColor, new Color(0.8f, 1f, 0.95f, 0.72f), pulse);
            return;
        }

        spriteRenderer.color = baseColor;

        if (Random.value < 0.006f)
            flickerTimer = 0.08f;
    }
}
