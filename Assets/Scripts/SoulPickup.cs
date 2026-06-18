using UnityEngine;

public class SoulPickup : MonoBehaviour
{
    [Header("Loại linh hồn")]
    public SoulType soulType = SoulType.Speed;

    [Header("Hiệu ứng lơ lửng")]
    public float floatSpeed = 1f;    // Tốc độ lơ lửng lên xuống
    public float floatHeight = 0.2f; // Biên độ lên xuống

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Tô màu theo loại linh hồn
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        switch (soulType)
        {
            case SoulType.Speed:
                sr.color = new Color(0f, 1f, 0.5f);   // Xanh lá
                break;
            case SoulType.Power:
                sr.color = new Color(1f, 0.2f, 0.2f); // Đỏ
                break;
            case SoulType.Defense:
                sr.color = new Color(0.2f, 0.5f, 1f); // Xanh dương
                break;
        }
    }

    void Update()
    {
        // Hiệu ứng lơ lửng lên xuống bằng hàm sin
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}