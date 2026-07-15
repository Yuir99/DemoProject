using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Visual Animation")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] downSprites;
    public Sprite[] leftSprites;
    public Sprite[] rightSprites;
    public Sprite[] upSprites;
    public float walkAnimationFps = 8f;
    public Transform muzzlePoint;
    public float muzzleDistance = 0.42f;

    [Header("Turret Placement")]
    public GameObject turretPrefab;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private MapBounds2D mapBounds;
    private float walkAnimationTime;
    private FacingDirection facingDirection = FacingDirection.Down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        muzzlePoint ??= transform.Find("GunTip");

        transform.rotation = Quaternion.identity;
        IgnorePlayerEnemyCollisions();
        mapBounds = FindFirstObjectByType<MapBounds2D>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        UpdateFacingTowardMouse();
        UpdateWalkAnimation();
        HandleTurretPlacement();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector2 nextPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        mapBounds ??= FindFirstObjectByType<MapBounds2D>();
        if (mapBounds != null)
            nextPosition = mapBounds.ClampPoint(nextPosition, mapBounds.playerPadding);

        rb.MovePosition(nextPosition);
    }

    public void UpgradeMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    void UpdateWalkAnimation()
    {
        if (spriteRenderer == null)
            return;

        Sprite[] frames = GetFacingSprites();
        if (frames == null || frames.Length == 0)
            return;

        if (moveInput.sqrMagnitude <= 0.01f)
            walkAnimationTime = 0f;
        else
            walkAnimationTime += Time.deltaTime;

        int frame = moveInput.sqrMagnitude <= 0.01f
            ? 0
            : Mathf.FloorToInt(walkAnimationTime * Mathf.Max(1f, walkAnimationFps)) % frames.Length;

        if (frames[frame] != null)
            spriteRenderer.sprite = frames[frame];
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

        if (mapBounds != null)
        {
            Vector2 clamped = mapBounds.ClampPoint(spawnPosition, mapBounds.playerPadding);
            spawnPosition.x = clamped.x;
            spawnPosition.y = clamped.y;
        }

        Instantiate(turretPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Placed turret at " + spawnPosition);
    }

    void UpdateFacingTowardMouse()
    {
        transform.rotation = Quaternion.identity;

        if (Camera.main == null || spriteRenderer == null)
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPos - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            facingDirection = direction.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        else
            facingDirection = direction.y < 0f ? FacingDirection.Down : FacingDirection.Up;

        spriteRenderer.flipX = false;
        if (muzzlePoint != null)
            muzzlePoint.localPosition = direction.normalized * muzzleDistance;
    }

    Sprite[] GetFacingSprites()
    {
        return facingDirection switch
        {
            FacingDirection.Left => leftSprites,
            FacingDirection.Right => rightSprites,
            FacingDirection.Up => upSprites,
            _ => downSprites
        };
    }

    void IgnorePlayerEnemyCollisions()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer >= 0 && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }

    enum FacingDirection { Down, Left, Right, Up }
}
