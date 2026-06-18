using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpUpgradeUI : MonoBehaviour
{
    private PlayerStats stats;
    private SoulGun soulGun;
    private PlayerController controller;
    private GameObject panel;
    private Text title;
    private readonly List<Button> buttons = new List<Button>();
    private int pendingChoices;

    void Update()
    {
        if (panel == null || !panel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            buttons[0].onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            buttons[1].onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            buttons[2].onClick.Invoke();
    }

    private struct UpgradeOption
    {
        public string name;
        public string description;
        public Action apply;

        public UpgradeOption(string name, string description, Action apply)
        {
            this.name = name;
            this.description = description;
            this.apply = apply;
        }
    }

    void Start()
    {
        EnsureEventSystem();
        BindPlayer();
        BuildUI();
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    void OnDestroy()
    {
        if (stats != null)
            stats.LeveledUp -= OnLevelUp;
    }

    void BindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        stats = player.GetComponent<PlayerStats>();
        soulGun = player.GetComponent<SoulGun>();
        controller = player.GetComponent<PlayerController>();

        if (stats != null)
            stats.LeveledUp += OnLevelUp;
    }

    void OnLevelUp(int newLevel)
    {
        pendingChoices++;
        if (!panel.activeSelf)
            ShowChoices(newLevel);
    }

    void ShowChoices(int level)
    {
        List<UpgradeOption> pool = BuildUpgradePool();
        for (int i = 0; i < buttons.Count; i++)
        {
            int pick = UnityEngine.Random.Range(0, pool.Count);
            UpgradeOption option = pool[pick];
            pool.RemoveAt(pick);

            Text label = buttons[i].GetComponentInChildren<Text>();
            label.text = option.name + "\n" + option.description;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => Choose(option));
        }

        title.text = "LEVEL " + level + " - CHOOSE AN UPGRADE";
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    List<UpgradeOption> BuildUpgradePool()
    {
        return new List<UpgradeOption>
        {
            new UpgradeOption("SOUL POWER", "+5 bullet damage", () => soulGun?.UpgradeBulletDamage(5f)),
            new UpgradeOption("RAPID FIRE", "Shoot 12% faster", () => soulGun?.UpgradeFireRate(0.88f)),
            new UpgradeOption("VITAL CORE", "+20 max HP and heal 20", () => stats?.UpgradeMaxHealth(20f)),
            new UpgradeOption("SWIFT STEP", "+0.4 movement speed", () => controller?.UpgradeMoveSpeed(0.4f)),
            new UpgradeOption("SOUL MAGNET", "+0.75 suck range and force", () => soulGun?.UpgradeSoulSuck(0.75f, 1f)),
            new UpgradeOption("RECOVERY", "Restore 35 HP", () => stats?.Heal(35f))
        };
    }

    void Choose(UpgradeOption option)
    {
        option.apply?.Invoke();
        pendingChoices = Mathf.Max(0, pendingChoices - 1);

        if (pendingChoices > 0)
            ShowChoices(stats == null ? 1 : stats.Level);
        else
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void BuildUI()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Level Up Panel");
        panel.transform.SetParent(transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.01f, 0.015f, 0.03f, 0.88f);

        title = CreateText(panel.transform, font, new Vector2(0f, 230f), new Vector2(900f, 70f), 34);

        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObject = new GameObject("Upgrade " + (i + 1));
            buttonObject.transform.SetParent(panel.transform, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 180f);
            rect.anchoredPosition = new Vector2((i - 1) * 400f, 0f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.09f, 0.13f, 0.2f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.18f, 0.32f, 0.42f, 1f);
            colors.pressedColor = new Color(0.12f, 0.55f, 0.5f, 1f);
            button.colors = colors;

            Text text = CreateText(buttonObject.transform, font, Vector2.zero, new Vector2(326f, 156f), 24);
            text.alignment = TextAnchor.MiddleCenter;
            buttons.Add(button);
        }

        panel.SetActive(false);
    }

    Text CreateText(Transform parent, Font font, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
