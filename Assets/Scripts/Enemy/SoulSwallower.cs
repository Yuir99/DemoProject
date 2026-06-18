using UnityEngine;

// Quái Nuốt Hồn: ưu tiên săn linh hồn trên bản đồ thay vì đi thẳng tới Lõi.
// Mỗi linh hồn bị nuốt sẽ hồi máu, tăng kích thước và tăng sát thương cho nó.
public class SoulSwallower : EnemyBase
{
    [Header("Soul Swallower")]
    public float soulSenseRange = 8f;
    public float consumeDistance = 0.55f;
    public float healPerSoul = 18f;
    public float maxGrowthBonus = 0.45f;

    // Trạng thái nội bộ dùng để theo dõi mục tiêu và số linh hồn đã ăn.
    private SoulPickup soulTarget;
    private int consumedSouls;
    private float nextSearchTime;
    private Vector3 baseScale;

    // Thiết lập chỉ số, loại linh hồn rơi ra và màu tím tạm thời.
    protected override void Start()
    {
        base.Start();

        maxHP = 90f;
        currentHP = maxHP;
        moveSpeed = 2.15f;
        damage = 12f;
        soulDropType = SoulType.Defense;
        soulDropCount = 1;
        xpReward = 10f;
        baseScale = transform.localScale;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = new Color(0.72f, 0.25f, 0.95f);
    }

    // Mỗi 0.3 giây tìm lại linh hồn gần nhất.
    // Nếu không có linh hồn, gọi EnemyBase để tiếp tục tấn công Lõi.
    protected override void Update()
    {
        if (Time.time >= nextSearchTime)
        {
            FindNearestSoul();
            nextSearchTime = Time.time + 0.3f;
        }

        if (soulTarget != null)
        {
            MoveToward(soulTarget.transform.position);
            if (Vector2.Distance(transform.position, soulTarget.transform.position) <= consumeDistance)
                ConsumeSoul();
            return;
        }

        base.Update();
    }

    // Duyệt các SoulPickup trong Scene và chọn linh hồn gần nhất trong tầm cảm nhận.
    void FindNearestSoul()
    {
        SoulPickup[] souls = FindObjectsByType<SoulPickup>(FindObjectsSortMode.None);
        SoulPickup nearest = null;
        float nearestSqrDistance = soulSenseRange * soulSenseRange;

        foreach (SoulPickup soul in souls)
        {
            if (soul == null)
                continue;

            float sqrDistance = (soul.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = soul;
            }
        }

        soulTarget = nearest;
    }

    // Di chuyển trực tiếp về một điểm với tốc độ hiện tại của quái.
    void MoveToward(Vector3 destination)
    {
        Vector2 direction = (destination - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

    // Xóa linh hồn, hồi máu và tăng sức mạnh sau khi đến đủ gần.
    void ConsumeSoul()
    {
        if (soulTarget == null)
            return;

        Destroy(soulTarget.gameObject);
        soulTarget = null;
        consumedSouls++;
        currentHP = Mathf.Min(maxHP, currentHP + healPerSoul);

        float growth = Mathf.Min(consumedSouls * 0.05f, maxGrowthBonus);
        transform.localScale = baseScale * (1f + growth);
        damage = 12f + consumedSouls * 1.5f;
    }

    // Vẽ vòng tròn tầm cảm nhận trong Scene khi chọn quái để dễ cân chỉnh.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.75f, 0.25f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, soulSenseRange);
    }
}
