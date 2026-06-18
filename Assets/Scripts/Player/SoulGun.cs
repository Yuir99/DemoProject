using UnityEngine;

// Quản lý bắn đạn, hút linh hồn, kho linh hồn và nạp linh hồn vào trụ.
public class SoulGun : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform gunTip;
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;
    public bool autoFire = true;

    [Header("Soul Suck")]
    public float suckRange = 4f;
    public float suckForce = 10f;
    public bool allowQToSuck = true;

    [Header("Feed Turret")]
    public KeyCode feedTurretKey = KeyCode.F;
    public float feedTurretRange = 10f;

    [Header("Soul Inventory")]
    [SerializeField] private int speedSouls = 0;
    [SerializeField] private int powerSouls = 0;
    [SerializeField] private int defenseSouls = 0;

    private float nextFireTime = 0f;
    private int selectedSoulType = 1;

    // Dữ liệu chỉ đọc để HUD hiển thị số linh hồn và loại đang chọn.
    public int SpeedSouls => speedSouls;
    public int PowerSouls => powerSouls;
    public int DefenseSouls => defenseSouls;
    public SoulType SelectedSoulType => GetSelectedSoulType();

    // Các hàm nâng cấp được LevelUpUpgradeUI gọi khi người chơi chọn phần thưởng.
    public void UpgradeFireRate(float multiplier)
    {
        fireRate = Mathf.Max(0.08f, fireRate * multiplier);
    }

    public void UpgradeBulletDamage(float amount)
    {
        if (bulletPrefab == null)
            return;

        Bullet bullet = bulletPrefab.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage += amount;
    }

    public void UpgradeSoulSuck(float rangeAmount, float forceAmount)
    {
        suckRange += rangeAmount;
        suckForce += forceAmount;
    }

    // Kiểm tra toàn bộ thao tác chiến đấu của người chơi mỗi frame.
    void Update()
    {
        HandleShooting();
        HandleSoulSuck();
        HandleSoulTypeSelect();
        HandleFeedTurret();
    }

    // Tự động bắn hoặc bắn khi nhận nút Fire1, có giới hạn bởi fireRate.
    void HandleShooting()
    {
        bool wantsToShoot = autoFire || Input.GetButton("Fire1");
        if (wantsToShoot && Time.time >= nextFireTime)
        {
            ShootBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Tạo đạn tại GunTip và cho đạn bay về vị trí chuột.
    void ShootBullet()
    {
        if (bulletPrefab == null || gunTip == null)
            return;

        Vector2 shootDirection = gunTip.up;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = gunTip.position.z;
            Vector2 toMouse = mouseWorldPos - gunTip.position;
            if (toMouse.sqrMagnitude > 0.001f)
                shootDirection = toMouse.normalized;
        }

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
        GameObject newBullet = Instantiate(bulletPrefab, gunTip.position, Quaternion.Euler(0f, 0f, angle));

        Rigidbody2D bulletRb = newBullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = shootDirection * bulletSpeed;

        Destroy(newBullet, 3f);
    }

    // Khi giữ chuột phải hoặc Q, kéo các linh hồn trong suckRange về người chơi.
    void HandleSoulSuck()
    {
        bool isSucking = Input.GetMouseButton(1) || (allowQToSuck && Input.GetKey(KeyCode.Q));
        if (!isSucking)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, suckRange, LayerMask.GetMask("Soul"));

        foreach (Collider2D hit in hits)
        {
            SoulPickup soul = hit.GetComponent<SoulPickup>();
            Rigidbody2D soulRb = hit.GetComponent<Rigidbody2D>();
            if (soul == null || soulRb == null)
                continue;

            Vector2 direction = (transform.position - hit.transform.position).normalized;
            soulRb.linearVelocity = direction * suckForce;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < 0.5f)
            {
                CollectSoul(soul.soulType);
                Destroy(hit.gameObject);
            }
        }
    }

    // Cộng linh hồn vừa hút vào đúng ngăn Speed, Power hoặc Defense.
    void CollectSoul(SoulType type)
    {
        switch (type)
        {
            case SoulType.Speed:
                speedSouls++;
                break;
            case SoulType.Power:
                powerSouls++;
                break;
            case SoulType.Defense:
                defenseSouls++;
                break;
        }

        Debug.Log($"Collected {type}. Speed={speedSouls} Power={powerSouls} Defense={defenseSouls}");
    }

    // Phím 1, 2, 3 lần lượt chọn Speed, Power và Defense Soul.
    void HandleSoulTypeSelect()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedSoulType = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            selectedSoulType = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            selectedSoulType = 3;
    }

    // Khi nhấn F, tìm trụ dưới con trỏ hoặc trên hướng ngắm rồi nạp một linh hồn.
    void HandleFeedTurret()
    {
        if (!Input.GetKeyDown(feedTurretKey) || Camera.main == null)
            return;

        int turretMask = LayerMask.GetMask("Turret");
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        Collider2D turretCollider = Physics2D.OverlapPoint(mousePos, turretMask);
        if (turretCollider == null)
        {
            Vector2 dir = (mousePos - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, feedTurretRange, turretMask);
            turretCollider = hit.collider;
        }

        if (turretCollider == null)
            return;

        TurretNode turret = turretCollider.GetComponentInParent<TurretNode>();
        if (turret == null)
            return;

        SoulType type = GetSelectedSoulType();
        if (GetSoulCount(type) <= 0)
        {
            Debug.Log($"No {type} souls in inventory.");
            return;
        }

        turret.ReceiveSoul(type);
        DeductSoul(type);
        Debug.Log($"Fed 1 {type} soul into turret.");
    }

    // Chuyển số lựa chọn nội bộ thành enum SoulType.
    SoulType GetSelectedSoulType()
    {
        switch (selectedSoulType)
        {
            case 2:
                return SoulType.Power;
            case 3:
                return SoulType.Defense;
            default:
                return SoulType.Speed;
        }
    }

    // Trả về số lượng hiện có của một loại linh hồn.
    int GetSoulCount(SoulType type)
    {
        switch (type)
        {
            case SoulType.Speed:
                return speedSouls;
            case SoulType.Power:
                return powerSouls;
            case SoulType.Defense:
                return defenseSouls;
            default:
                return 0;
        }
    }

    // Trừ một linh hồn sau khi nạp thành công vào trụ.
    void DeductSoul(SoulType type)
    {
        switch (type)
        {
            case SoulType.Speed:
                speedSouls = Mathf.Max(0, speedSouls - 1);
                break;
            case SoulType.Power:
                powerSouls = Mathf.Max(0, powerSouls - 1);
                break;
            case SoulType.Defense:
                defenseSouls = Mathf.Max(0, defenseSouls - 1);
                break;
        }
    }

    // Vẽ tầm hút linh hồn trong Scene để dễ cân chỉnh.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, suckRange);
    }
}

// Ba loại linh hồn hiện có trong trò chơi.
public enum SoulType { Speed, Power, Defense }
