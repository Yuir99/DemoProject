using System.Collections;
using UnityEngine;

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

    protected Transform targetTransform;

    private float baseMoveSpeed;
    private Coroutine slowRoutine;
    private Coroutine burnRoutine;
    private Coroutine knockbackRoutine;
    private bool isDead;

    protected virtual void Start()
    {
        currentHP = maxHP;
        baseMoveSpeed = moveSpeed;

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

    protected virtual void Update()
    {
        MoveTowardTarget();
    }

    protected virtual void MoveTowardTarget()
    {
        if (targetTransform == null)
            return;

        Vector2 dir = (targetTransform.position - transform.position).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead)
            return;

        currentHP -= amount;
        StartCoroutine(DamageFlash());

        if (currentHP <= 0f)
            Die();
    }

    public void ApplyBurn(float damagePerSecond, float duration)
    {
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration));
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (slowRoutine == null)
            baseMoveSpeed = moveSpeed;

        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, force));
    }

    public void ApplyDifficulty(float healthMultiplier, float damageMultiplier, float speedMultiplier)
    {
        maxHP *= healthMultiplier;
        currentHP = maxHP;
        damage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
        baseMoveSpeed = moveSpeed;
        xpReward *= Mathf.Lerp(1f, healthMultiplier, 0.5f);
    }

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

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

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

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.RegisterEnemyKilled(xpReward);

        Destroy(gameObject);
    }

    IEnumerator BurnRoutine(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && currentHP > 0f)
        {
            TakeDamage(damagePerSecond * 0.5f);
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        burnRoutine = null;
    }

    IEnumerator SlowRoutine(float multiplier, float duration)
    {
        moveSpeed = baseMoveSpeed * Mathf.Clamp(multiplier, 0.1f, 1f);
        yield return new WaitForSeconds(duration);
        moveSpeed = baseMoveSpeed;
        slowRoutine = null;
    }

    IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        float timer = 0f;
        const float duration = 0.12f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.Translate(direction * force * Time.deltaTime, Space.World);
            yield return null;
        }

        knockbackRoutine = null;
    }

    IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            yield break;

        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        sr.color = original;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EnergyCore"))
            other.GetComponent<EnergyCore>()?.TakeDamage(damage * Time.deltaTime);
    }
}
