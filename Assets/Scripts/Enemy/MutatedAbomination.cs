using UnityEngine;

public class MutatedAbomination : EnemyBase
{
    public GameObject bossCoreDropPrefab;

    public void Initialize(int speedSouls, int powerSouls, int defenseSouls)
    {
        moveSpeed = 1.5f + (speedSouls / 3f) * 1.5f;
        damage = 20f + (powerSouls / 3f) * 25f;
        maxHP = 300f + defenseSouls * 25f;
        currentHP = maxHP;

        float scale = 1.15f + defenseSouls * 0.04f;
        transform.localScale = Vector3.one * scale;

        Debug.Log($"Mutated turret: HP={maxHP} | Speed={moveSpeed:F1} | DMG={damage:F0}");
    }

    protected override void Die()
    {
        if (bossCoreDropPrefab != null)
            Instantiate(bossCoreDropPrefab, transform.position, Quaternion.identity);

        Debug.Log("Mutated turret destroyed.");
        base.Die();
    }
}
