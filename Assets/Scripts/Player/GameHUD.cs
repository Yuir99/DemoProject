using UnityEngine;
using UnityEngine.UI;

// Tạo và cập nhật HUD chính: máu, XP, linh hồn, máu Lõi, thời gian và wave.
public class GameHUD : MonoBehaviour
{
    // Tham chiếu dữ liệu gameplay và các thành phần UI cần cập nhật.
    private PlayerStats stats;
    private SoulGun soulGun;
    private Image hpFill;
    private Image xpFill;
    private Text hpText;
    private Text xpText;
    private Text soulText;
    private EnergyCore energyCore;
    private Image coreFill;
    private Text coreText;
    private WaveSpawner waveSpawner;
    private Text timerText;

    // Dựng toàn bộ HUD ngay khi GameObject được tạo.
    void Awake()
    {
        BuildHUD();
    }

    // Mỗi frame đọc dữ liệu mới nhất rồi cập nhật chữ và độ đầy của các thanh.
    void Update()
    {
        BindPlayer();

        if (stats != null)
        {
            hpFill.fillAmount = Mathf.Clamp01(stats.HealthPercent);
            xpFill.fillAmount = Mathf.Clamp01(stats.XPPercent);
            hpText.text = $"HP {Mathf.CeilToInt(stats.CurrentHP)}/{Mathf.CeilToInt(stats.maxHP)}";
            xpText.text = $"LV {stats.Level}  XP {Mathf.FloorToInt(stats.CurrentXP)}/{Mathf.CeilToInt(stats.xpToNextLevel)}";
        }

        if (soulGun != null)
        {
            soulText.text = $"Souls  Speed {soulGun.SpeedSouls}   Power {soulGun.PowerSouls}   Defense {soulGun.DefenseSouls}   Selected {soulGun.SelectedSoulType}";
        }

        BindCore();
        if (energyCore != null)
        {
            coreFill.fillAmount = Mathf.Clamp01(energyCore.HealthPercent);
            coreText.text = $"ENERGY CORE  {Mathf.CeilToInt(energyCore.currentHP)}/{Mathf.CeilToInt(energyCore.maxHP)}";
        }

        BindSpawner();
        if (waveSpawner != null)
        {
            int totalSeconds = Mathf.CeilToInt(waveSpawner.RemainingTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = waveSpawner.finalBossSpawned
                ? "FINAL BOSS"
                : $"{minutes:00}:{seconds:00}   WAVE {waveSpawner.currentWave}";
        }
    }

    // Tự tìm Lõi Trung Tâm nếu chưa có tham chiếu.
    void BindCore()
    {
        if (energyCore != null)
            return;

        energyCore = FindFirstObjectByType<EnergyCore>();
    }

    // Tự tìm WaveSpawner để lấy thời gian còn lại và số wave.
    void BindSpawner()
    {
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();
    }

    // Tự tìm Player và hai component chứa máu/XP cùng kho linh hồn.
    void BindPlayer()
    {
        if (stats != null && soulGun != null)
            return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        if (stats == null)
            stats = player.GetComponent<PlayerStats>();

        if (soulGun == null)
            soulGun = player.GetComponent<SoulGun>();
    }

    // Tạo Canvas và bố trí các nhóm HUD trên màn hình.
    void BuildHUD()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform root = canvas.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        RectTransform panel = CreatePanel(root, "HUD Panel", new Vector2(18f, -18f), new Vector2(430f, 154f));

        hpText = CreateText(panel, "HP Text", font, new Vector2(0f, -2f), "HP");
        hpFill = CreateBar(panel, "HP Bar", new Vector2(0f, -30f), new Color(0.95f, 0.18f, 0.18f));

        xpText = CreateText(panel, "XP Text", font, new Vector2(0f, -58f), "XP");
        xpFill = CreateBar(panel, "XP Bar", new Vector2(0f, -86f), new Color(0.25f, 0.65f, 1f));

        soulText = CreateText(panel, "Soul Text", font, new Vector2(0f, -122f), "Souls");
        soulText.fontSize = 18;

        RectTransform corePanel = CreatePanel(root, "Core Health Panel", new Vector2(0f, -18f), new Vector2(620f, 72f));
        corePanel.anchorMin = new Vector2(0.5f, 1f);
        corePanel.anchorMax = new Vector2(0.5f, 1f);
        corePanel.pivot = new Vector2(0.5f, 1f);
        coreText = CreateText(corePanel, "Core HP Text", font, new Vector2(0f, -4f), "ENERGY CORE");
        coreText.alignment = TextAnchor.MiddleCenter;
        coreFill = CreateBar(corePanel, "Core HP Bar", new Vector2(0f, -34f), new Color(1f, 0.68f, 0.08f));

        timerText = CreateText(root, "Match Timer", font, new Vector2(-18f, -18f), "15:00");
        RectTransform timerRect = timerText.rectTransform;
        timerRect.anchorMin = new Vector2(1f, 1f);
        timerRect.anchorMax = new Vector2(1f, 1f);
        timerRect.pivot = new Vector2(1f, 1f);
        timerRect.sizeDelta = new Vector2(300f, 44f);
        timerText.alignment = TextAnchor.MiddleRight;
        timerText.fontSize = 28;
    }

    // Hàm hỗ trợ tạo một panel nền tối.
    RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);

        RectTransform rect = panelObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image background = panelObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.025f, 0.04f, 0.72f);

        return rect;
    }

    // Hàm hỗ trợ tạo một dòng chữ trong HUD.
    Text CreateText(RectTransform parent, string name, Font font, Vector2 anchoredPosition, string value)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(-22f, 24f);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.text = value;

        return text;
    }

    // Hàm hỗ trợ tạo thanh nền và phần fill có thể thay đổi từ 0 đến 1.
    Image CreateBar(RectTransform parent, string name, Vector2 anchoredPosition, Color fillColor)
    {
        GameObject backgroundObject = new GameObject(name + " Background");
        backgroundObject.transform.SetParent(parent, false);

        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(1f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = anchoredPosition;
        backgroundRect.sizeDelta = new Vector2(-22f, 16f);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.16f);

        GameObject fillObject = new GameObject(name + " Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillObject.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;

        return fill;
    }
}
