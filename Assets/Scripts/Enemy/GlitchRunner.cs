using UnityEngine;

// Quái Tốc Độ: ít máu, chạy nhanh, sát thương thấp và rơi Speed Soul.
public class GlitchRunner : EnemyBase
{
    // Giữ SpriteRenderer để tạo hiệu ứng chớp tắt.
    private SpriteRenderer spriteRenderer;

    // Thiết lập bộ chỉ số, màu tạm và hitbox riêng của Glitch Runner.
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

    // Tiếp tục di chuyển bằng EnemyBase và thỉnh thoảng chớp tắt như bị glitch.
    protected override void Update()
    {
        base.Update();

        if (spriteRenderer != null && Random.value < 0.01f)
            spriteRenderer.enabled = !spriteRenderer.enabled;
    }
}
