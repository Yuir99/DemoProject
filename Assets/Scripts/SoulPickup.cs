using UnityEngine;

// Đại diện cho một linh hồn rơi ra sau khi quái bị tiêu diệt.
// Script đặt màu theo loại linh hồn và tạo hiệu ứng lơ lửng.
public class SoulPickup : MonoBehaviour
{
    [Header("Loại linh hồn")]
    public SoulType soulType = SoulType.Speed;

    [Header("Hiệu ứng lơ lửng")]
    public float floatSpeed = 1f;
    public float floatHeight = 0.2f;

    // Vị trí ban đầu dùng làm tâm của chuyển động lên xuống.
    private Vector3 startPos;

    // Lưu vị trí và tô màu ngay khi linh hồn xuất hiện.
    void Start()
    {
        startPos = transform.position;

        // Mỗi loại linh hồn có một màu nhận diện riêng.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        switch (soulType)
        {
            case SoulType.Speed:
                sr.color = new Color(0f, 1f, 0.5f);
                break;
            case SoulType.Power:
                sr.color = new Color(1f, 0.2f, 0.2f);
                break;
            case SoulType.Defense:
                sr.color = new Color(0.2f, 0.5f, 1f);
                break;
        }
    }

    // Dùng hàm sin để di chuyển linh hồn lên xuống mềm mại theo thời gian.
    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
