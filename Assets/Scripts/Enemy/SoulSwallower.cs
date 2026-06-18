using UnityEngine;

public class SoulSwallower : EnemyBase
{
    [Header("Soul Swallower")]
    public float soulSenseRange = 8f;
    public float consumeDistance = 0.55f;
    public float healPerSoul = 18f;
    public float maxGrowthBonus = 0.45f;

    private SoulPickup soulTarget;
    private int consumedSouls;
    private float nextSearchTime;
    private Vector3 baseScale;

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

    void MoveToward(Vector3 destination)
    {
        Vector2 direction = (destination - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.75f, 0.25f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, soulSenseRange);
    }
}
