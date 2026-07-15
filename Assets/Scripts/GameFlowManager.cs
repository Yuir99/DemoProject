using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public static GameMode PendingMode => GameSession.SelectedMode;
    public static bool IsEndlessMode => Instance != null && Instance.CurrentMode == GameMode.Endless;

    public GameMode CurrentMode { get; private set; } = GameMode.Timed;
    public bool GameIsRunning { get; private set; }
    public bool GameHasEnded { get; private set; }
    public int EnemyKills { get; private set; }
    public int EndlessScore { get; private set; }
    public float SurvivalTime { get; private set; }

    private WaveSpawner waveSpawner;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        GameMode requestedMode = GameSession.SelectedMode;
        StartMode(requestedMode == GameMode.Menu ? GameMode.Timed : requestedMode);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!GameIsRunning || GameHasEnded)
            return;

        SurvivalTime += Time.deltaTime;
        if (CurrentMode == GameMode.Endless)
            EndlessScore = CalculateEndlessScore();
    }

    public void StartMode(GameMode mode)
    {
        CurrentMode = mode;
        GameIsRunning = true;
        GameHasEnded = false;
        EnemyKills = 0;
        EndlessScore = 0;
        SurvivalTime = 0f;
        Time.timeScale = 1f;

        waveSpawner ??= FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null)
            waveSpawner.BeginMode(mode);
    }

    public void RegisterEnemyKilled(float xpReward)
    {
        if (!GameIsRunning || GameHasEnded)
            return;

        EnemyKills++;
        if (CurrentMode == GameMode.Endless)
            EndlessScore = CalculateEndlessScore();
    }

    public void WinTimedMode()
    {
        if (CurrentMode == GameMode.Timed)
            EndGame(true);
    }

    public void LoseGame()
    {
        EndGame(false);
    }

    void EndGame(bool won)
    {
        if (GameHasEnded)
            return;

        GameHasEnded = true;
        GameIsRunning = false;
        EndlessScore = CurrentMode == GameMode.Endless ? CalculateEndlessScore() : 0;

        GameSession.CompleteRun(CurrentMode, won, EndlessScore, SurvivalTime, EnemyKills);
        Time.timeScale = 1f;
        SceneManager.LoadScene("ResultScene");
    }

    int CalculateEndlessScore()
    {
        int wave = waveSpawner == null ? 0 : waveSpawner.currentWave;
        return Mathf.FloorToInt(SurvivalTime * 10f) + EnemyKills * 75 + wave * 150;
    }

    public static string FormatTime(float seconds)
    {
        return GameSession.FormatTime(seconds);
    }
}
