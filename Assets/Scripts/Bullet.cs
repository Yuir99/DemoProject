using UnityEngine;

// Điều khiển viên đạn của người chơi và trụ.
// Khi collider của đạn chạm collider của quái, đạn gây sát thương rồi tự hủy.
public class Bullet : MonoBehaviour
{
    // Lượng máu bị trừ trong một lần đạn trúng mục tiêu.
    public float damage = 20f;

    // Unity tự gọi hàm này khi đạn đi vào một Collider2D được đặt là Trigger.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra vật vừa chạm có script EnemyBase hay không.
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            // Gây sát thương cho quái và xóa viên đạn khỏi Scene.
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
