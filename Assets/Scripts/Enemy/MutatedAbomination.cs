using UnityEngine;

public class MutatedAbomination : EnemyBase
{
    public GameObject bossCoreDropPrefab; // Lõi Kháng Thể (kéo vào)

    // Hàm này được gọi từ TurretNode khi trụ Overmutate
    public void Initialize(int speedSouls, int powerSouls, int defenseSouls)
    {
        // Tính chỉ số từ linh hồn đã nạp
        moveSpeed = 1.5f + (speedSouls / 3f) * 1.5f;
        damage = 20f + (powerSouls / 3f) * 25f;
        maxHP = 300f + defenseSouls * 25f;
        currentHP = maxHP;

        // Kích thước tỉ lệ với số hồn phòng thủ
        float scale = 1.8f + defenseSouls * 0.08f;
        transform.localScale = Vector3.one * scale;

        Debug.Log($"🐉 Quái Đột Biến: HP={maxHP} | Speed={moveSpeed:F1} | DMG={damage:F0}");
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().color = new Color(0.4f, 0f, 0.8f);
    }

    protected override void Die()
    {
        // Rớt Lõi Kháng Thể thay vì linh hồn thường
        if (bossCoreDropPrefab != null)
            Instantiate(bossCoreDropPrefab, transform.position, Quaternion.identity);

        Debug.Log("💀 Quái Đột Biến bị tiêu diệt! Rơi Lõi Kháng Thể!");
        Destroy(gameObject);
    }
}