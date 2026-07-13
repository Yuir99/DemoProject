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

    void Update()
    {
        if (panel == null || !panel.activeSelf)
            return;

        if (buttons.Count > 0 && buttons[0].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha1))
            buttons[0].onClick.Invoke();
        else if (buttons.Count > 1 && buttons[1].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha2))
            buttons[1].onClick.Invoke();
        else if (buttons.Count > 2 && buttons[2].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha3))
            buttons[2].onClick.Invoke();
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
        bool choosingWeaponPath = ShouldShowWeaponPathChoice(level);
        List<UpgradeOption> pool = choosingWeaponPath ? BuildWeaponPathPool() : BuildUpgradePool();
        int activeButtonCount = Mathf.Min(buttons.Count, pool.Count);
        LayoutButtons(activeButtonCount);

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].gameObject.SetActive(i < activeButtonCount);
            if (i >= activeButtonCount)
                continue;

            int pick = UnityEngine.Random.Range(0, pool.Count);
            UpgradeOption option = pool[pick];
            pool.RemoveAt(pick);

            Text label = buttons[i].GetComponentInChildren<Text>();
            label.text = option.name + "\n" + option.description;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => Choose(option));
        }

        title.text = choosingWeaponPath
            ? "LEVEL " + level + " - CHOOSE YOUR WEAPON"
            : "LEVEL " + level + " - CHOOSE AN UPGRADE";
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    bool ShouldShowWeaponPathChoice(int level)
    {
        return level >= 2 && soulGun != null && !soulGun.HasChosenWeaponPath;
    }

    List<UpgradeOption> BuildWeaponPathPool()
    {
        return new List<UpgradeOption>
        {
            new UpgradeOption(
                "RIFLE PATH",
                "Straight fast shots\n+fire rate, +move speed",
                () =>
                {
                    soulGun?.ChooseRiflePath();
                    controller?.UpgradeMoveSpeed(0.8f);
                }),
            new UpgradeOption(
                "SHOTGUN PATH",
                "Cone spread, shorter range\nclose shots hit harder",
                () =>
                {
                    soulGun?.ChooseShotgunPath();
                    controller?.UpgradeMoveSpeed(0.2f);
                })
        };
    }

    List<UpgradeOption> BuildUpgradePool()
    {
        return new List<UpgradeOption>
        {
            new UpgradeOption("WEAPON TRAINING", "Weapon level +1\n+damage, +rate, +range", () => soulGun?.UpgradeWeaponLevel()),
            new UpgradeOption("SOUL POWER", "+5 damage dealt", () => soulGun?.UpgradeBulletDamage(5f)),
            new UpgradeOption("RAPID FIRE", "Shoot 12% faster", () => soulGun?.UpgradeFireRate(0.88f)),
            new UpgradeOption("SWIFT STEP", "+0.4 movement speed", () => controller?.UpgradeMoveSpeed(0.4f)),
            new UpgradeOption("LONG BARREL", "+1.0 weapon range", () => soulGun?.UpgradeBulletRange(1f)),
            new UpgradeOption("PIERCING SHOT", "Bullets pierce +1 enemy", () => soulGun?.UpgradePierce(1)),
            new UpgradeOption("KNOCKBACK ROUND", "Bullets push enemies back", () => soulGun?.UpgradeKnockback(2.4f))
        };
    }

    void LayoutButtons(int activeButtonCount)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            if (rect == null)
                continue;

            if (activeButtonCount == 2)
                rect.anchoredPosition = new Vector2((i - 0.5f) * 430f, 0f);
            else
                rect.anchoredPosition = new Vector2((i - 1) * 400f, 0f);
        }
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
