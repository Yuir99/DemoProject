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

    [Header("Mutated Enemy")]
    public GameObject mutatedAbominationPrefab;

    [Header("UI")]
    public Image[] soulSlots;
    public GameObject warningIcon;

    private readonly Color colorSpeed = new Color(0f, 1f, 0.5f);
    private readonly Color colorPower = new Color(1f, 0.2f, 0.2f);
    private readonly Color colorDefense = new Color(0.2f, 0.5f, 1f);
    private readonly Color colorEmpty = new Color(0.3f, 0.3f, 0.3f);

    private SpriteRenderer bodySprite;
    private TurretShooter shooter;
    private int TotalSouls => speedSouls + powerSouls + defenseSouls;

    void Awake()
    {
        bodySprite = transform.Find("Body")?.GetComponent<SpriteRenderer>();
        shooter = GetComponent<TurretShooter>();
        if (shooter == null)
            shooter = gameObject.AddComponent<TurretShooter>();

        AutoWireUI();

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;
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
        CheckMilestones();

        if (TotalSouls >= 3 && shooter != null)
            shooter.Activate();

        if (TotalSouls >= 9)
            CheckOvermutate();
    }

    void CheckMilestones()
    {
        if (bodySprite == null)
            return;

        if (powerSouls == 0 && defenseSouls == 0)
        {
            if (speedSouls == 3)
            {
                ApplySpriteOrColor(spriteSpeedLv3, colorSpeed);
            }
            else if (speedSouls == 6)
            {
                ApplySpriteOrColor(spriteSpeedLv6, new Color(0f, 0.9f, 0.3f));
            }
            else if (speedSouls == 9)
            {
                ApplySpriteOrColor(spriteLaserTurret, new Color(0.5f, 1f, 0f));
                transform.localScale = Vector3.one * 1.3f;
            }
        }

        if (speedSouls == 0 && defenseSouls == 0)
        {
            if (powerSouls == 3)
            {
                ApplySpriteOrColor(spritePowerLv3, new Color(1f, 0.5f, 0f));
            }
            else if (powerSouls == 6)
            {
                ApplySpriteOrColor(spritePowerLv6, new Color(1f, 0.2f, 0f));
            }
            else if (powerSouls == 9)
            {
                ApplySpriteOrColor(spriteMortarTurret, new Color(0.8f, 0f, 0f));
                transform.localScale = Vector3.one * 1.5f;
            }
        }
    }

    void CheckOvermutate()
    {
        int typesUsed = (speedSouls > 0 ? 1 : 0)
                      + (powerSouls > 0 ? 1 : 0)
                      + (defenseSouls > 0 ? 1 : 0);

        if (typesUsed >= 2)
        {
            Debug.LogWarning("Overmutate: turret is turning into a mutant.");
            StartCoroutine(OvermutateSequence());
        }
        else
        {
            Debug.Log("Turret reached 9 pure souls safely.");
        }
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
            SetSlotColor(idx, colorSpeed);

        for (int i = 0; i < powerSouls && idx < soulSlots.Length; i++, idx++)
            SetSlotColor(idx, colorPower);

        for (int i = 0; i < defenseSouls && idx < soulSlots.Length; i++, idx++)
            SetSlotColor(idx, colorDefense);

        for (; idx < soulSlots.Length; idx++)
            SetSlotColor(idx, colorEmpty);

        int typesUsed = (speedSouls > 0 ? 1 : 0)
                      + (powerSouls > 0 ? 1 : 0)
                      + (defenseSouls > 0 ? 1 : 0);

        if (warningIcon != null)
            warningIcon.SetActive(TotalSouls >= 5 && typesUsed >= 2);
    }

    void SetSlotColor(int index, Color color)
    {
        if (soulSlots[index] != null)
            soulSlots[index].color = color;
    }

    void UpdateBodySprite()
    {
        if (bodySprite == null)
            return;

        if (spriteBase != null)
            bodySprite.sprite = spriteBase;

        if (bodySprite.color.a <= 0.01f)
            bodySprite.color = Color.white;
    }

    void ApplySpriteOrColor(Sprite sprite, Color fallbackColor)
    {
        if (bodySprite == null)
            return;

        if (sprite != null)
        {
            bodySprite.sprite = sprite;
            bodySprite.color = Color.white;
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
