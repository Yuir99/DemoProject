using UnityEngine;

// Lớp nền dùng chung cho mọi loại quái.
// Nó quản lý chỉ số, tìm Lõi, di chuyển, nhận sát thương, chết và rơi linh hồn.
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 50f;
    public float currentHP;
    public float moveSpeed = 2f;
    public float damage = 10f;

    [Header("Soul Drop")]
    public SoulType soulDropType = SoulType.Speed;
    public int soulDropCount = 1;
    public GameObject soulPrefab;
    public float xpReward = 5f;

    // Mục tiêu quái sẽ đi tới, ưu tiên Lõi rồi mới tới Player.
    protected Transform targetTransform;

    // Khởi tạo máu và tìm mục tiêu khi quái vừa xuất hiện.
    protected virtual void Start()
    {
        currentHP = maxHP;

        GameObject core = GameObject.FindWithTag("EnergyCore");
        if (core != null)
        {
            targetTransform = core.transform;
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            targetTransform = player.transform;
    }

    // Các lớp quái con có thể ghi đè Update nhưng vẫn gọi base.Update để di chuyển.
    protected virtual void Update()
    {
        MoveTowardTarget();
    }

    // Tính hướng chuẩn hóa rồi đưa quái tiến về mục tiêu theo moveSpeed.
    protected virtual void MoveTowardTarget()
    {
        if (targetTransform == null)
            return;

        Vector2 dir = (targetTransform.position - transform.position).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    // Nhận sát thương, chớp đỏ và chết khi máu về 0.
    public virtual void TakeDamage(float amount)
    {
        currentHP -= amount;
        StartCoroutine(DamageFlash());

        if (currentHP <= 0f)
            Die();
    }

    // Nhân chỉ số quái theo thời gian sống của màn chơi.
    public void ApplyDifficulty(float healthMultiplier, float damageMultiplier, float speedMultiplier)
    {
        maxHP *= healthMultiplier;
        currentHP = maxHP;
        damage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
        xpReward *= Mathf.Lerp(1f, healthMultiplier, 0.5f);
    }

    // Biến một quái thường thành mini boss hoặc boss cuối.
    public void ConfigureElite(string enemyName, float healthMultiplier, float damageMultiplier,
        float speedMultiplier, float scaleMultiplier, Color color, int extraSoulDrops)
    {
        gameObject.name = enemyName;
        ApplyDifficulty(healthMultiplier, damageMultiplier, speedMultiplier);
        transform.localScale *= scaleMultiplier;
        soulDropCount += extraSoulDrops;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = color;
    }

    // Tạo linh hồn, cộng XP cho người chơi rồi xóa quái.
    protected virtual void Die()
    {
        for (int i = 0; i < soulDropCount; i++)
        {
            Vector3 offset = (Vector3)Random.insideUnitCircle * 0.8f;
            Vector3 spawnPos = transform.position + offset;

            if (soulPrefab != null)
            {
                GameObject soul = Instantiate(soulPrefab, spawnPos, Quaternion.identity);
                SoulPickup pickup = soul.GetComponent<SoulPickup>();
                if (pickup != null)
                    pickup.soulType = soulDropType;
            }
        }

        GameObject player = GameObject.FindWithTag("Player");
        PlayerStats stats = player == null ? null : player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.GainXP(xpReward);

        Destroy(gameObject);
    }

    // Đổi màu quái sang đỏ trong thời gian ngắn khi bị bắn trúng.
    System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            yield break;

        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        sr.color = original;
    }

    // Khi đứng trong collider của Lõi, quái gây sát thương liên tục theo thời gian.
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EnergyCore"))
            other.GetComponent<EnergyCore>()?.TakeDamage(damage * Time.deltaTime);
    }
}
