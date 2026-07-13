using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretNode : MonoBehaviour
{
    [Header("Current Souls")]
    [SerializeField] private int speedSouls = 0;
    [SerializeField] private int powerSouls = 0;
    [SerializeField] private int defenseSouls = 0;

    [Header("State Sprites")]
    public Sprite spriteBase;
    public Sprite spriteSpeedLv3;
    public Sprite spriteSpeedLv6;
    public Sprite spriteLaserTurret;
    public Sprite spritePowerLv3;
    public Sprite spritePowerLv6;
    public Sprite spriteMortarTurret;

    [Header("Turret Animation")]
    public Sprite turretIdleSprite;
    public Sprite[] turretFireSprites;

    [Header("Resonance Sprites")]
    public Sprite flameTurretSprite;
    public Sprite frostTurretSprite;
    public Sprite stormTurretSprite;
    public Sprite sonicTurretSprite;
    public Sprite fireProjectileSprite;
    public Sprite iceProjectileSprite;
    public Sprite lightningProjectileSprite;

    [Header("Mutated Enemy")]
    public GameObject mutatedAbominationPrefab;

    [Header("UI")]
    public Image[] soulSlots;
    public GameObject warningIcon;
    public bool hideEmptySoulSlots = true;

    private readonly Color colorSpeed = new Color(0f, 1f, 0.5f);
    private readonly Color colorPower = new Color(1f, 0.2f, 0.2f);
    private readonly Color colorDefense = new Color(0.2f, 0.5f, 1f);
    private readonly Color colorEmpty = new Color(0.3f, 0.3f, 0.3f, 0.25f);

    private SpriteRenderer bodySprite;
    private TurretShooter shooter;
    private TurretResonance currentResonance = TurretResonance.None;

    private int TotalSouls => speedSouls + powerSouls + defenseSouls;

    void Awake()
    {
        Transform body = transform.Find("Body");
        bodySprite = body != null ? body.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();

        shooter = GetComponent<TurretShooter>();
        if (shooter == null)
            shooter = gameObject.AddComponent<TurretShooter>();

        AutoWireUI();
        CompactSoulUI();

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;

        shooter.ConfigureVisuals(bodySprite, GetBaseSprite(), turretFireSprites);
    }

    void Start()
    {
        UpdateBodySprite();
        UpdateSoulUI();
    }

    public void ReceiveSoul(SoulType type)
    {
        if (TotalSouls >= 9)
        {
            Debug.Log("Turret is already full.");
            return;
        }

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

        Debug.Log($"Turret received {type}. Speed={speedSouls} Power={powerSouls} Defense={defenseSouls}");

        UpdateSoulUI();
        TurretResonance matchedResonance = GetMatchedResonance();
        if (matchedResonance != TurretResonance.None)
            ApplyResonance(matchedResonance);
        else
            CheckMilestones();

        if (TotalSouls >= 3 && shooter != null)
            shooter.Activate();

        if (TotalSouls >= 9)
            ResolveFullSoulState();
    }

    void CheckMilestones()
    {
        if (bodySprite == null)
            return;

        if (powerSouls == 0 && defenseSouls == 0)
        {
            if (speedSouls == 3)
                ApplySpriteOrColor(spriteSpeedLv3, colorSpeed);
            else if (speedSouls == 6)
                ApplySpriteOrColor(spriteSpeedLv6, new Color(0f, 0.9f, 0.3f));
            else if (speedSouls == 9)
            {
                ApplySpriteOrColor(spriteLaserTurret, new Color(0.5f, 1f, 0f));
                transform.localScale = Vector3.one * 1.3f;
            }
        }

        if (speedSouls == 0 && defenseSouls == 0)
        {
            if (powerSouls == 3)
                ApplySpriteOrColor(spritePowerLv3, new Color(1f, 0.5f, 0f));
            else if (powerSouls == 6)
                ApplySpriteOrColor(spritePowerLv6, new Color(1f, 0.2f, 0f));
            else if (powerSouls == 9)
            {
                ApplySpriteOrColor(spriteMortarTurret, new Color(0.8f, 0f, 0f));
                transform.localScale = Vector3.one * 1.5f;
            }
        }
    }

    TurretResonance GetMatchedResonance()
    {
        if (TotalSouls < 9)
            return TurretResonance.None;

        if (powerSouls == 6 && speedSouls == 3 && defenseSouls == 0)
            return TurretResonance.Flame;

        if (speedSouls == 6 && defenseSouls == 3 && powerSouls == 0)
            return TurretResonance.Frost;

        if (speedSouls == 3 && powerSouls == 3 && defenseSouls == 3)
            return TurretResonance.Storm;

        if (defenseSouls == 6 && powerSouls == 3 && speedSouls == 0)
            return TurretResonance.Sonic;

        return TurretResonance.None;
    }

    void ApplyResonance(TurretResonance resonance)
    {
        if (currentResonance == resonance)
            return;

        currentResonance = resonance;
        Sprite selectedSprite = null;

        switch (resonance)
        {
            case TurretResonance.Flame:
                selectedSprite = flameTurretSprite;
                ApplySpriteOrColor(selectedSprite, new Color(1f, 0.35f, 0.08f));
                ConfigureShooter(TurretAttackMode.Bullet, 6.5f, 0.55f, 14f, 10f, fireProjectileSprite,
                    new Color(1f, 0.45f, 0.08f), 0.5f, 0, 0.5f, 6f, 2.5f, 1f, 0f);
                Debug.Log("Turret resonance: Flame. Formula = 6 Power + 3 Speed.");
                break;
            case TurretResonance.Frost:
                selectedSprite = frostTurretSprite;
                ApplySpriteOrColor(selectedSprite, new Color(0.35f, 0.85f, 1f));
                ConfigureShooter(TurretAttackMode.Bullet, 7f, 0.7f, 10f, 8.5f, iceProjectileSprite,
                    new Color(0.55f, 0.9f, 1f), 0.55f, 1, 0.2f, 0f, 0f, 0.55f, 2.2f);
                Debug.Log("Turret resonance: Frost. Formula = 6 Speed + 3 Defense.");
                break;
            case TurretResonance.Storm:
                selectedSprite = stormTurretSprite;
                ApplySpriteOrColor(selectedSprite, new Color(0.75f, 0.65f, 1f));
                ConfigureShooter(TurretAttackMode.Bullet, 8f, 0.38f, 9f, 14f, lightningProjectileSprite,
                    new Color(0.8f, 0.75f, 1f), 0.35f, 2, 0.4f, 0f, 0f, 1f, 0f);
                Debug.Log("Turret resonance: Storm. Formula = 3 Speed + 3 Power + 3 Defense.");
                break;
            case TurretResonance.Sonic:
                selectedSprite = sonicTurretSprite;
                ApplySpriteOrColor(selectedSprite, new Color(0.95f, 0.95f, 0.55f));
                ConfigureShooter(TurretAttackMode.SonicPulse, 3.4f, 0.9f, 11f, 0f, null,
                    new Color(1f, 0.95f, 0.55f), 1f, 0, 1.2f, 0f, 0f, 0.65f, 1.6f);
                Debug.Log("Turret resonance: Sonic. Formula = 6 Defense + 3 Power.");
                break;
        }

        transform.localScale = Vector3.one * 1.25f;
    }

    void ConfigureShooter(TurretAttackMode mode, float range, float fireRate, float damage, float bulletSpeed,
        Sprite projectileSprite, Color projectileColor, float projectileScale, int pierce, float knockback,
        float burnDamagePerSecond, float burnDuration, float slowMultiplier, float slowDuration)
    {
        if (shooter == null)
            return;

        shooter.Activate();
        shooter.ConfigureAttack(mode, range, fireRate, damage, bulletSpeed, projectileSprite, projectileColor, projectileScale,
            pierce, knockback, burnDamagePerSecond, burnDuration, slowMultiplier, slowDuration);
    }

    void ResolveFullSoulState()
    {
        if (currentResonance != TurretResonance.None)
        {
            Debug.Log("Turret reached a stable resonance.");
            return;
        }

        Debug.LogWarning("Unstable soul mix: turret is turning into a mutant.");
        StartCoroutine(OvermutateSequence());
    }

    IEnumerator OvermutateSequence()
    {
        float timer = 0f;
        Vector3 originalPos = transform.position;

        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            float shake = Mathf.Sin(timer * 30f) * 0.05f;
            transform.position = originalPos + new Vector3(shake, shake * 0.5f, 0f);
            yield return null;
        }

        if (mutatedAbominationPrefab != null)
        {
            GameObject monster = Instantiate(mutatedAbominationPrefab, originalPos, Quaternion.identity);
            MutatedAbomination ma = monster.GetComponent<MutatedAbomination>();
            if (ma != null)
                ma.Initialize(speedSouls, powerSouls, defenseSouls);
        }

        Destroy(gameObject);
    }

    void UpdateSoulUI()
    {
        if (soulSlots == null || soulSlots.Length == 0)
            AutoWireUI();

        if (soulSlots == null || soulSlots.Length == 0)
            return;

        int idx = 0;

        for (int i = 0; i < speedSouls && idx < soulSlots.Length; i++, idx++)
            SetSlot(idx, colorSpeed, true);

        for (int i = 0; i < powerSouls && idx < soulSlots.Length; i++, idx++)
            SetSlot(idx, colorPower, true);

        for (int i = 0; i < defenseSouls && idx < soulSlots.Length; i++, idx++)
            SetSlot(idx, colorDefense, true);

        for (; idx < soulSlots.Length; idx++)
            SetSlot(idx, colorEmpty, false);

        int typesUsed = (speedSouls > 0 ? 1 : 0)
                      + (powerSouls > 0 ? 1 : 0)
                      + (defenseSouls > 0 ? 1 : 0);

        if (warningIcon != null)
            warningIcon.SetActive(TotalSouls >= 5 && typesUsed >= 2);
    }

    void SetSlot(int index, Color color, bool filled)
    {
        if (soulSlots[index] == null)
            return;

        soulSlots[index].color = color;
        soulSlots[index].gameObject.SetActive(filled || !hideEmptySoulSlots);
    }

    void UpdateBodySprite()
    {
        if (bodySprite == null)
            return;

        Sprite baseSprite = GetBaseSprite();
        if (baseSprite != null)
        {
            bodySprite.sprite = baseSprite;
            bodySprite.color = Color.white;
        }
        else if (bodySprite.color.a <= 0.01f)
        {
            bodySprite.color = Color.white;
        }

        if (shooter != null)
            shooter.ConfigureVisuals(bodySprite, bodySprite.sprite, turretFireSprites);
    }

    Sprite GetBaseSprite()
    {
        if (turretIdleSprite != null)
            return turretIdleSprite;

        return spriteBase;
    }

    void ApplySpriteOrColor(Sprite sprite, Color fallbackColor)
    {
        if (bodySprite == null)
            return;

        if (sprite != null)
        {
            bodySprite.sprite = sprite;
            bodySprite.color = Color.white;

            if (shooter != null)
                shooter.idleSprite = sprite;
        }
        else
        {
            bodySprite.color = fallbackColor;
        }
    }

    void AutoWireUI()
    {
        List<Image> slots = new List<Image>();
        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image.name.StartsWith("Slot_"))
                slots.Add(image);
        }

        slots.Sort((a, b) => GetSlotIndex(a.name).CompareTo(GetSlotIndex(b.name)));

        if (slots.Count > 0)
            soulSlots = slots.ToArray();

        if (warningIcon == null)
        {
            Transform warning = transform.Find("SoulUI/WarningIcon");
            if (warning != null)
                warningIcon = warning.gameObject;
        }
    }

    void CompactSoulUI()
    {
        Transform ui = transform.Find("SoulUI");
        if (ui != null)
        {
            ui.localScale = Vector3.one * 0.008f;
            ui.localPosition = new Vector3(0f, 0.48f, 0f);
        }

        if (soulSlots != null)
        {
            for (int i = 0; i < soulSlots.Length; i++)
            {
                RectTransform slotRect = soulSlots[i]?.GetComponent<RectTransform>();
                if (slotRect == null)
                    continue;

                slotRect.anchoredPosition = new Vector2((i - 4) * 5f, 0f);
                slotRect.sizeDelta = new Vector2(5f, 5f);
            }
        }

        if (warningIcon != null)
        {
            RectTransform warningRect = warningIcon.GetComponent<RectTransform>();
            if (warningRect != null)
            {
                warningRect.anchoredPosition = new Vector2(0f, 9f);
                warningRect.sizeDelta = new Vector2(22f, 22f);
            }
        }
    }

    int GetSlotIndex(string slotName)
    {
        string number = slotName.Replace("Slot_", "");
        int index;
        return int.TryParse(number, out index) ? index : 999;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}

public enum TurretResonance { None, Flame, Frost, Storm, Sonic }
