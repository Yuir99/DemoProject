using UnityEngine;

public class GlitchRunner : EnemyBase
{
    private SpriteRenderer spriteRenderer;

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
            spriteRenderer.color = new Color(0.5f, 1f, 0.5f);

        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        if (hitbox != null)
            hitbox.radius = 0.38f;
    }

    protected override void Update()
    {
        base.Update();

        if (spriteRenderer != null && Random.value < 0.01f)
            spriteRenderer.enabled = !spriteRenderer.enabled;
    }
}
