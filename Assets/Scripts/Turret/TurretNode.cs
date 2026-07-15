using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretNode : MonoBehaviour
{
    [Header("Current Souls")]
    [SerializeField] private int speedSouls;
    [SerializeField] private int powerSouls;
    [SerializeField] private int defenseSouls;

    [Header("Separated Visual Parts")]
    public SpriteRenderer baseRenderer;
    public SpriteRenderer headRenderer;
    public SpriteRenderer resonanceEffectRenderer;

    [Header("Construction")]
    public Sprite[] constructionSprites;
    public float constructionFrameDuration = 0.14f;

    [Header("Basic Head Animation")]
    public Sprite[] basicIdleSprites;
    public Sprite[] basicFireSprites;

    [Header("Flame Head Animation")]
    public Sprite[] flameIdleSprites;
    public Sprite[] flameFireSprites;
    public Sprite[] flameEffectSprites;

    [Header("Frost Head Animation")]
    public Sprite[] frostIdleSprites;
    public Sprite[] frostFireSprites;
    public Sprite[] frostEffectSprites;

    [Header("Storm Head Animation")]
    public Sprite[] stormIdleSprites;
    public Sprite[] stormFireSprites;
    public Sprite[] stormEffectSprites;

    [Header("Sonic Head Animation")]
    public Sprite[] sonicIdleSprites;
    public Sprite[] sonicFireSprites;
    public Sprite[] sonicEffectSprites;

    [Header("Corrupted Animation")]
    public Sprite[] corruptedIdleSprites;
    public Sprite[] corruptedFireSprites;

    [Header("Projectiles")]
    public Sprite fireProjectileSprite;
    public Sprite iceProjectileSprite;
    public Sprite lightningProjectileSprite;

    [Header("Mutated Enemy")]
    public GameObject mutatedAbominationPrefab;

    [Header("UI")]
    public Image[] soulSlots;
    public GameObject warningIcon;
    public bool hideEmptySoulSlots = true;
    public Sprite speedSoulIcon;
    public Sprite powerSoulIcon;
    public Sprite defenseSoulIcon;

    [Header("Animation Timing")]
    public float resonanceEffectFrameDuration = 0.08f;

    private readonly Color colorSpeed = new(0f, 1f, 0.5f);
    private readonly Color colorPower = new(1f, 0.2f, 0.2f);
    private readonly Color colorDefense = new(0.2f, 0.5f, 1f);
    private readonly Color colorEmpty = new(0.3f, 0.3f, 0.3f, 0.25f);

    private TurretShooter shooter;
    private TurretResonance currentResonance = TurretResonance.None;
    private Coroutine resonanceEffectRoutine;

    private int TotalSouls => speedSouls + powerSouls + defenseSouls;

    void Awake()
    {
        AutoWireVisuals();
        AutoWireUI();

        shooter = GetComponent<TurretShooter>();
        if (shooter == null)
        {
            Debug.LogError("Turret prefab needs a TurretShooter component.", this);
            enabled = false;
            return;
        }

        shooter.rotatingPart = headRenderer != null ? headRenderer.transform : null;
        shooter.turretRenderer = headRenderer;
        if (headRenderer != null)
            shooter.muzzlePoint = headRenderer.transform.Find("MuzzlePoint");

        ApplyHeadAnimation(basicIdleSprites, basicFireSprites);

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    void Start()
    {
        UpdateSoulUI();

        if (constructionSprites != null && constructionSprites.Length > 0)
            StartCoroutine(PlayConstructionAnimation());
        else
            ShowCompletedTurret();
    }

    IEnumerator PlayConstructionAnimation()
    {
        if (headRenderer != null)
            headRenderer.enabled = false;

        foreach (Sprite frame in constructionSprites)
        {
            if (baseRenderer != null && frame != null)
                baseRenderer.sprite = frame;

            yield return new WaitForSeconds(Mathf.Max(0.04f, constructionFrameDuration));
        }

        ShowCompletedTurret();
    }

    void ShowCompletedTurret()
    {
        if (baseRenderer != null && constructionSprites != null && constructionSprites.Length > 0)
            baseRenderer.sprite = constructionSprites[^1];

        if (headRenderer != null)
            headRenderer.enabled = true;

        if (currentResonance == TurretResonance.None)
            ApplyHeadAnimation(basicIdleSprites, basicFireSprites);
        else
            ApplyResonance(currentResonance, true);
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

        UpdateSoulUI();

        if (TotalSouls >= 3 && shooter != null)
            shooter.Activate();

        if (TotalSouls >= 9)
            ResolveFullSoulState();
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

    void ResolveFullSoulState()
    {
        TurretResonance matched = GetMatchedResonance();
        if (matched != TurretResonance.None)
        {
            ApplyResonance(matched);
            return;
        }

        StartCoroutine(OvermutateSequence());
    }

    void ApplyResonance(TurretResonance resonance, bool force = false)
    {
        if (!force && currentResonance == resonance)
            return;

        currentResonance = resonance;
        Sprite[] idleFrames = basicIdleSprites;
        Sprite[] fireFrames = basicFireSprites;
        Sprite[] effectFrames = null;

        switch (resonance)
        {
            case TurretResonance.Flame:
                idleFrames = flameIdleSprites;
                fireFrames = flameFireSprites;
                effectFrames = flameEffectSprites;
                ConfigureShooter(TurretAttackMode.Bullet, 6.5f, 0.55f, 14f, 10f, fireProjectileSprite,
                    new Color(1f, 0.45f, 0.08f), 0.5f, 0, 0.5f, 6f, 2.5f, 1f, 0f);
                break;

            case TurretResonance.Frost:
                idleFrames = frostIdleSprites;
                fireFrames = frostFireSprites;
                effectFrames = frostEffectSprites;
                ConfigureShooter(TurretAttackMode.Bullet, 7f, 0.7f, 10f, 8.5f, iceProjectileSprite,
                    new Color(0.55f, 0.9f, 1f), 0.55f, 1, 0.2f, 0f, 0f, 0.55f, 2.2f);
                break;

            case TurretResonance.Storm:
                idleFrames = stormIdleSprites;
                fireFrames = stormFireSprites;
                effectFrames = stormEffectSprites;
                ConfigureShooter(TurretAttackMode.Bullet, 8f, 0.38f, 9f, 14f, lightningProjectileSprite,
                    new Color(0.8f, 0.75f, 1f), 0.35f, 2, 0.4f, 0f, 0f, 1f, 0f);
                break;

            case TurretResonance.Sonic:
                idleFrames = sonicIdleSprites;
                fireFrames = sonicFireSprites;
                effectFrames = sonicEffectSprites;
                ConfigureShooter(TurretAttackMode.SonicPulse, 3.4f, 0.9f, 11f, 0f, null,
                    new Color(1f, 0.95f, 0.55f), 1f, 0, 1.2f, 0f, 0f, 0.65f, 1.6f);
                break;
        }

        ApplyHeadAnimation(idleFrames, fireFrames);
        PlayResonanceEffect(effectFrames);
    }

    void ApplyHeadAnimation(Sprite[] idleFrames, Sprite[] fireFrames)
    {
        if (headRenderer == null || shooter == null)
            return;

        headRenderer.color = Color.white;
        shooter.ConfigureVisuals(headRenderer, idleFrames, fireFrames);
    }

    void ConfigureShooter(TurretAttackMode mode, float range, float fireRate, float damage, float bulletSpeed,
        Sprite projectileSprite, Color projectileColor, float projectileScale, int pierce, float knockback,
        float burnDamagePerSecond, float burnDuration, float slowMultiplier, float slowDuration)
    {
        if (shooter == null)
            return;

        shooter.Activate();
        shooter.ConfigureAttack(mode, range, fireRate, damage, bulletSpeed, projectileSprite, projectileColor,
            projectileScale, pierce, knockback, burnDamagePerSecond, burnDuration, slowMultiplier, slowDuration);
    }

    void PlayResonanceEffect(Sprite[] frames)
    {
        if (resonanceEffectRenderer == null || frames == null || frames.Length == 0)
            return;

        if (resonanceEffectRoutine != null)
            StopCoroutine(resonanceEffectRoutine);

        resonanceEffectRoutine = StartCoroutine(PlayEffectFrames(frames));
    }

    IEnumerator PlayEffectFrames(Sprite[] frames)
    {
        resonanceEffectRenderer.enabled = true;
        resonanceEffectRenderer.color = Color.white;

        foreach (Sprite frame in frames)
        {
            if (frame != null)
                resonanceEffectRenderer.sprite = frame;

            yield return new WaitForSeconds(Mathf.Max(0.04f, resonanceEffectFrameDuration));
        }

        resonanceEffectRenderer.enabled = false;
        resonanceEffectRoutine = null;
    }

    IEnumerator OvermutateSequence()
    {
        ApplyHeadAnimation(corruptedIdleSprites, corruptedFireSprites);

        float timer = 0f;
        Vector3 originalPosition = transform.position;
        while (timer < 1.2f)
        {
            timer += Time.deltaTime;
            float shake = Mathf.Sin(timer * 32f) * 0.04f;
            transform.position = originalPosition + new Vector3(shake, shake * 0.5f, 0f);
            yield return null;
        }

        transform.position = originalPosition;

        if (mutatedAbominationPrefab != null)
        {
            GameObject monster = Instantiate(mutatedAbominationPrefab, originalPosition, Quaternion.identity);
            MutatedAbomination mutant = monster.GetComponent<MutatedAbomination>();
            if (mutant != null)
                mutant.Initialize(speedSouls, powerSouls, defenseSouls);
        }

        Destroy(gameObject);
    }

    void AutoWireVisuals()
    {
        if (baseRenderer == null)
        {
            Transform basePart = transform.Find("Base");
            if (basePart != null)
                baseRenderer = basePart.GetComponent<SpriteRenderer>();
        }

        if (headRenderer == null)
        {
            Transform head = transform.Find("RotatingHead");
            if (head != null)
                headRenderer = head.GetComponent<SpriteRenderer>();
        }

        if (resonanceEffectRenderer == null)
        {
            Transform effect = transform.Find("ResonanceEffect");
            if (effect != null)
                resonanceEffectRenderer = effect.GetComponent<SpriteRenderer>();
        }
    }

    void UpdateSoulUI()
    {
        if (soulSlots == null || soulSlots.Length == 0)
            AutoWireUI();

        if (soulSlots == null || soulSlots.Length == 0)
            return;

        int index = 0;

        for (int i = 0; i < speedSouls && index < soulSlots.Length; i++, index++)
            SetSlot(index, colorSpeed, speedSoulIcon, true);

        for (int i = 0; i < powerSouls && index < soulSlots.Length; i++, index++)
            SetSlot(index, colorPower, powerSoulIcon, true);

        for (int i = 0; i < defenseSouls && index < soulSlots.Length; i++, index++)
            SetSlot(index, colorDefense, defenseSoulIcon, true);

        for (; index < soulSlots.Length; index++)
            SetSlot(index, colorEmpty, null, false);

        int typesUsed = (speedSouls > 0 ? 1 : 0)
                      + (powerSouls > 0 ? 1 : 0)
                      + (defenseSouls > 0 ? 1 : 0);

        if (warningIcon != null)
            warningIcon.SetActive(TotalSouls >= 5 && typesUsed >= 2);
    }

    void SetSlot(int index, Color fallbackColor, Sprite icon, bool filled)
    {
        if (soulSlots[index] == null)
            return;

        soulSlots[index].sprite = icon;
        soulSlots[index].preserveAspect = true;
        soulSlots[index].color = icon != null ? Color.white : fallbackColor;
        soulSlots[index].gameObject.SetActive(filled || !hideEmptySoulSlots);
    }

    void AutoWireUI()
    {
        List<Image> slots = new();
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

    int GetSlotIndex(string slotName)
    {
        return int.TryParse(slotName.Replace("Slot_", ""), out int index) ? index : 999;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}

public enum TurretResonance { None, Flame, Frost, Storm, Sonic }
