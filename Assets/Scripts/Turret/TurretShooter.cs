using UnityEngine;

// Khả năng chiến đấu của trụ sau khi trụ đạt ít nhất 3 linh hồn.
public class TurretShooter : MonoBehaviour
{
    // Các chỉ số tầm bắn, nhịp bắn, tốc độ đạn và sát thương đạn.
    public float range = 6f;
    public float fireRate = 0.65f;
    public float bulletSpeed = 10f;
    public float bulletDamage = 12f;
    public bool isActive = false;

    private GameObject bulletPrefab;
    private float nextFireTime;

    // Khi trụ hoạt động và đã hết thời gian chờ, tìm quái gần nhất để bắn.
    void Update()
    {
        if (!isActive || Time.time < nextFireTime)
            return;

        EnemyBase target = FindNearestEnemy();
        if (target == null)
            return;

        Shoot(target);
        nextFireTime = Time.time + fireRate;
    }

    // TurretNode gọi hàm này khi trụ đạt mốc 3 linh hồn.
    public void Activate()
    {
        isActive = true;
    }

    // Duyệt tất cả quái và chọn quái gần nhất nằm trong range.
    EnemyBase FindNearestEnemy()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        EnemyBase nearest = null;
        float nearestSqrDistance = range * range;

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    // Tạo viên đạn, đặt sát thương và cho đạn bay về phía mục tiêu.
    void Shoot(EnemyBase target)
    {
        EnsureBulletPrefab();
        if (bulletPrefab == null)
            return;

        Vector2 direction = (target.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0f, 0f, angle));

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.damage = bulletDamage;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;

        Destroy(bullet, 3f);
    }

    // Dùng chung Bullet prefab của người chơi để chưa cần tạo prefab đạn riêng cho trụ.
    void EnsureBulletPrefab()
    {
        if (bulletPrefab != null)
            return;

        SoulGun soulGun = FindFirstObjectByType<SoulGun>();
        if (soulGun != null)
            bulletPrefab = soulGun.bulletPrefab;
    }

    // Vẽ vòng tròn tầm bắn trong Scene khi chọn trụ.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
