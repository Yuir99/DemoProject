using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;
    public float maxTravelDistance = 0f;
    public float closeDamageMultiplier = 1f;
    public float farDamageMultiplier = 1f;

    [Header("Ammo Upgrades")]
    public int pierceCount = 0;
    public float knockbackForce = 0f;

    [Header("Status Effects")]
    public float burnDamagePerSecond = 0f;
    public float burnDuration = 0f;
    public float slowMultiplier = 1f;
    public float slowDuration = 0f;

    private readonly HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();
    private Vector3 spawnPosition;

    void Awake()
    {
        spawnPosition = transform.position;
    }

    void Update()
    {
        if (maxTravelDistance <= 0f)
            return;

        if (Vector3.Distance(spawnPosition, transform.position) >= maxTravelDistance)
            Destroy(gameObject);
    }

    public void Configure(float newDamage, float newMaxTravelDistance = 0f, float newCloseMultiplier = 1f, float newFarMultiplier = 1f,
        int newPierceCount = 0, float newKnockbackForce = 0f, float newBurnDamagePerSecond = 0f, float newBurnDuration = 0f,
        float newSlowMultiplier = 1f, float newSlowDuration = 0f)
    {
        damage = newDamage;
        maxTravelDistance = newMaxTravelDistance;
        closeDamageMultiplier = newCloseMultiplier;
        farDamageMultiplier = newFarMultiplier;
        pierceCount = Mathf.Max(0, newPierceCount);
        knockbackForce = Mathf.Max(0f, newKnockbackForce);
        burnDamagePerSecond = Mathf.Max(0f, newBurnDamagePerSecond);
        burnDuration = Mathf.Max(0f, newBurnDuration);
        slowMultiplier = Mathf.Clamp(newSlowMultiplier, 0.1f, 1f);
        slowDuration = Mathf.Max(0f, newSlowDuration);
        spawnPosition = transform.position;
        hitEnemies.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);
        enemy.TakeDamage(GetDamageAtCurrentDistance());

        if (knockbackForce > 0f)
            enemy.ApplyKnockback(GetTravelDirection(), knockbackForce);

        if (burnDamagePerSecond > 0f && burnDuration > 0f)
            enemy.ApplyBurn(burnDamagePerSecond, burnDuration);

        if (slowMultiplier < 1f && slowDuration > 0f)
            enemy.ApplySlow(slowMultiplier, slowDuration);

        if (pierceCount <= 0)
            Destroy(gameObject);
        else
            pierceCount--;
    }

    float GetDamageAtCurrentDistance()
    {
        if (maxTravelDistance <= 0f)
            return damage * closeDamageMultiplier;

        float traveled = Vector3.Distance(spawnPosition, transform.position);
        float t = Mathf.Clamp01(traveled / maxTravelDistance);
        float multiplier = Mathf.Lerp(closeDamageMultiplier, farDamageMultiplier, t);
        return damage * multiplier;
    }

    Vector2 GetTravelDirection()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.001f)
            return rb.linearVelocity.normalized;

        return transform.up;
    }
}
