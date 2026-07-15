using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button[] upgradeButtons;

    private PlayerStats stats;
    private SoulGun soulGun;
    private PlayerController controller;
    private int pendingChoices;

    private struct UpgradeOption
    {
        public readonly string name;
        public readonly string description;
        public readonly Action apply;

        public UpgradeOption(string name, string description, Action apply)
        {
            this.name = name;
            this.description = description;
            this.apply = apply;
        }
    }

    void Start()
    {
        BindSceneUI();
        BindPlayer();

        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf || upgradeButtons == null)
            return;

        if (upgradeButtons.Length > 0 && upgradeButtons[0].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha1))
            upgradeButtons[0].onClick.Invoke();
        else if (upgradeButtons.Length > 1 && upgradeButtons[1].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha2))
            upgradeButtons[1].onClick.Invoke();
        else if (upgradeButtons.Length > 2 && upgradeButtons[2].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha3))
            upgradeButtons[2].onClick.Invoke();
    }

    void OnDestroy()
    {
        if (stats != null)
            stats.LeveledUp -= OnLevelUp;
    }

    void BindSceneUI()
    {
        panel ??= SceneObjectLookup.FindGameObject("LevelUpPanel");
        title ??= SceneObjectLookup.FindComponent<TMP_Text>("LevelUpTitle");

        if (upgradeButtons == null || upgradeButtons.Length != 3)
        {
            upgradeButtons = new[]
            {
                SceneObjectLookup.FindComponent<Button>("UpgradeButton1"),
                SceneObjectLookup.FindComponent<Button>("UpgradeButton2"),
                SceneObjectLookup.FindComponent<Button>("UpgradeButton3")
            };
        }
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
        if (panel != null && !panel.activeSelf)
            ShowChoices(newLevel);
    }

    void ShowChoices(int level)
    {
        bool choosingWeaponPath = ShouldShowWeaponPathChoice(level);
        List<UpgradeOption> pool = choosingWeaponPath ? BuildWeaponPathPool() : BuildUpgradePool();
        int activeButtonCount = Mathf.Min(upgradeButtons.Length, pool.Count);
        LayoutButtons(activeButtonCount);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            Button button = upgradeButtons[i];
            if (button == null)
                continue;

            button.gameObject.SetActive(i < activeButtonCount);
            if (i >= activeButtonCount)
                continue;

            int pick = UnityEngine.Random.Range(0, pool.Count);
            UpgradeOption option = pool[pick];
            pool.RemoveAt(pick);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = option.name + "\n" + option.description;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Choose(option));
        }

        if (title != null)
        {
            title.text = choosingWeaponPath
                ? $"LEVEL {level} - CHOOSE YOUR WEAPON"
                : $"LEVEL {level} - CHOOSE AN UPGRADE";
        }

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
            new UpgradeOption("RIFLE PATH", "Straight fast shots\n+fire rate, +move speed", () =>
            {
                soulGun?.ChooseRiflePath();
                controller?.UpgradeMoveSpeed(0.8f);
            }),
            new UpgradeOption("SHOTGUN PATH", "Cone spread, shorter range\nclose shots hit harder", () =>
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
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] == null)
                continue;

            RectTransform rect = upgradeButtons[i].GetComponent<RectTransform>();
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
}
