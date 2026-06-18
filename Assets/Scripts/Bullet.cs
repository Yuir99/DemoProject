using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Khi đạn chạm vào quái
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Xóa đạn
        }
    }
}
