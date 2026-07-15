using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private Image xpFill;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text soulText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private Image coreFill;
    [SerializeField] private TMP_Text coreText;
    [SerializeField] private TMP_Text timerText;

    private PlayerStats stats;
    private SoulGun soulGun;
    private EnergyCore energyCore;
    private WaveSpawner waveSpawner;

    void Awake()
    {
        BindSceneUI();
    }

    void Update()
    {
        BindPlayer();

        if (stats != null)
        {
            if (hpFill != null)
                hpFill.fillAmount = Mathf.Clamp01(stats.HealthPercent);
            if (xpFill != null)
                xpFill.fillAmount = Mathf.Clamp01(stats.XPPercent);
            if (hpText != null)
                hpText.text = $"HP {Mathf.CeilToInt(stats.CurrentHP)}/{Mathf.CeilToInt(stats.maxHP)}";
            if (xpText != null)
                xpText.text = $"LV {stats.Level}  XP {Mathf.FloorToInt(stats.CurrentXP)}/{Mathf.CeilToInt(stats.xpToNextLevel)}";
        }

        if (soulGun != null)
        {
            if (soulText != null)
                soulText.text = $"Souls  Speed {soulGun.SpeedSouls}   Power {soulGun.PowerSouls}   Defense {soulGun.DefenseSouls}   Selected {soulGun.SelectedSoulType}";
            if (weaponText != null)
                weaponText.text = $"Weapon {soulGun.WeaponDisplayName}  LV {soulGun.WeaponLevel}";
        }

        energyCore ??= FindFirstObjectByType<EnergyCore>();
        if (energyCore != null)
        {
            if (coreFill != null)
                coreFill.fillAmount = Mathf.Clamp01(energyCore.HealthPercent);
            if (coreText != null)
                coreText.text = $"ENERGY CORE  {Mathf.CeilToInt(energyCore.currentHP)}/{Mathf.CeilToInt(energyCore.maxHP)}";
        }

        waveSpawner ??= FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null && timerText != null)
        {
            string timeText = GameSession.FormatTime(waveSpawner.RemainingTime);
            timerText.text = waveSpawner.IsEndless && GameFlowManager.Instance != null
                ? $"ENDLESS {timeText}   WAVE {waveSpawner.currentWave}   SCORE {GameFlowManager.Instance.EndlessScore}"
                : $"{timeText}   WAVE {waveSpawner.currentWave}";
        }
    }

    void BindSceneUI()
    {
        hpFill ??= SceneObjectLookup.FindComponent<Image>("HPBarFill");
        xpFill ??= SceneObjectLookup.FindComponent<Image>("XPBarFill");
        hpText ??= SceneObjectLookup.FindComponent<TMP_Text>("HPText");
        xpText ??= SceneObjectLookup.FindComponent<TMP_Text>("XPText");
        soulText ??= SceneObjectLookup.FindComponent<TMP_Text>("SoulText");
        weaponText ??= SceneObjectLookup.FindComponent<TMP_Text>("WeaponText");
        coreFill ??= SceneObjectLookup.FindComponent<Image>("CoreBarFill");
        coreText ??= SceneObjectLookup.FindComponent<TMP_Text>("CoreHPText");
        timerText ??= SceneObjectLookup.FindComponent<TMP_Text>("MatchTimerText");
    }

    void BindPlayer()
    {
        if (stats != null && soulGun != null)
            return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        stats ??= player.GetComponent<PlayerStats>();
        soulGun ??= player.GetComponent<SoulGun>();
    }
}
