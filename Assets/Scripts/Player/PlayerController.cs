using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Turret Placement")]
    public GameObject turretPrefab;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        HandleTurretPlacement();
        RotateTowardMouse();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    public void UpgradeMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

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
