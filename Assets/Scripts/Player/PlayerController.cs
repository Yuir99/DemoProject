using UnityEngine;

// Xử lý di chuyển, xoay theo chuột và đặt trụ của người chơi.
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Turret Placement")]
    public GameObject turretPrefab;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Lấy Rigidbody2D để di chuyển bằng hệ thống vật lý 2D.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Đọc bàn phím, xử lý đặt trụ và cập nhật hướng nhìn mỗi frame.
    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        HandleTurretPlacement();
        RotateTowardMouse();
    }

    // Di chuyển trong FixedUpdate để Rigidbody2D hoạt động ổn định.
    void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // Được hệ thống nâng cấp gọi để cộng thêm tốc độ chạy.
    public void UpgradeMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    // Khi nhấn E, tạo một trụ tại vị trí con trỏ chuột trong thế giới game.
    void HandleTurretPlacement()
    {
        if (!Input.GetKeyDown(KeyCode.E) || turretPrefab == null)
            return;

        Vector3 spawnPosition = transform.position;
        if (Camera.main != null)
        {
            spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spawnPosition.z = transform.position.z;
        }

        Instantiate(turretPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Placed turret at " + spawnPosition);
    }

    // Chuyển vị trí chuột từ màn hình sang thế giới rồi xoay nhân vật về hướng đó.
    void RotateTowardMouse()
    {
        if (Camera.main == null)
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPos - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
