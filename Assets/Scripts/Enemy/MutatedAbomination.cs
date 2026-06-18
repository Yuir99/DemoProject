using UnityEngine;

// Quái đột biến sinh ra khi một trụ nạp đủ 9 linh hồn nhưng trộn nhiều loại.
public class MutatedAbomination : EnemyBase
{
    // Vật phẩm đặc biệt có thể rơi ra khi quái đột biến bị tiêu diệt.
    public GameObject bossCoreDropPrefab;

    // TurretNode truyền số lượng từng loại linh hồn vào đây để tính chỉ số quái.
    public void Initialize(int speedSouls, int powerSouls, int defenseSouls)
    {
        // Speed Soul tăng tốc độ, Power Soul tăng sát thương, Defense Soul tăng máu.
        moveSpeed = 1.5f + (speedSouls / 3f) * 1.5f;
        damage = 20f + (powerSouls / 3f) * 25f;
        maxHP = 300f + defenseSouls * 25f;
        currentHP = maxHP;

        // Nhiều Defense Soul cũng khiến quái có kích thước lớn hơn.
        float scale = 1.8f + defenseSouls * 0.08f;
        transform.localScale = Vector3.one * scale;

        Debug.Log($"Quái đột biến: HP={maxHP} | Speed={moveSpeed:F1} | DMG={damage:F0}");
    }

    // Chạy khởi tạo chung và đặt màu tím tạm thời cho quái.
    protected override void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().color = new Color(0.4f, 0f, 0.8f);
    }

    // Ghi đè cách chết thông thường để không rơi linh hồn thường.
    protected override void Die()
    {
        // Nếu đã gán prefab, quái sẽ rơi Lõi Kháng Thể.
        if (bossCoreDropPrefab != null)
            Instantiate(bossCoreDropPrefab, transform.position, Quaternion.identity);

        Debug.Log("Quái đột biến đã bị tiêu diệt.");
        Destroy(gameObject);
    }
}
