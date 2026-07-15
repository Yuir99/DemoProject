using UnityEngine;

public class SoulPickup : MonoBehaviour
{
    [Header("Soul Type")]
    public SoulType soulType = SoulType.Speed;
    public Sprite speedSprite;
    public Sprite powerSprite;
    public Sprite defenseSprite;

    [Header("Hover Animation")]
    public float floatSpeed = 2.4f;
    public float floatHeight = 0.12f;
    public float pulseAmount = 0.06f;

    private Vector3 startPos;
    private Vector3 baseScale;
    private float phaseOffset;

    void Start()
    {
        startPos = transform.position;
        baseScale = transform.localScale;
        phaseOffset = Random.value * Mathf.PI * 2f;
        ApplyVisual();
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * floatSpeed + phaseOffset);
        transform.position = startPos + Vector3.up * (wave * floatHeight);
        transform.localScale = baseScale * (1f + wave * pulseAmount);
    }

    void ApplyVisual()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        renderer.color = Color.white;
        Sprite selected = soulType switch
        {
            SoulType.Power => powerSprite,
            SoulType.Defense => defenseSprite,
            _ => speedSprite
        };

        if (selected != null)
            renderer.sprite = selected;
    }
}
