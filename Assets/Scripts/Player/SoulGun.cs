using UnityEngine;

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

    public int SpeedSouls => speedSouls;
    public int PowerSouls => powerSouls;
    public int DefenseSouls => defenseSouls;
    public SoulType SelectedSoulType => GetSelectedSoulType();

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

    void Update()
    {
        HandleShooting();
        HandleSoulSuck();
        HandleSoulTypeSelect();
        HandleFeedTurret();
    }

    void HandleShooting()
    {
        bool wantsToShoot = autoFire || Input.GetButton("Fire1");
        if (wantsToShoot && Time.time >= nextFireTime)
        {
            ShootBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

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

    void HandleSoulTypeSelect()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedSoulType = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            selectedSoulType = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            selectedSoulType = 3;
    }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, suckRange);
    }
}

public enum SoulType { Speed, Power, Defense }
