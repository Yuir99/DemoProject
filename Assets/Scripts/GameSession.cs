using UnityEngine;

public enum GameMode
{
    Menu,
    Timed,
    Endless
}

public static class GameSession
{
    private const string SelectedModeKey = "SelectedGameMode";
    private const string HasResultKey = "RunHasResult";
    private const string ResultModeKey = "RunMode";
    private const string ResultWonKey = "RunWon";
    private const string ResultScoreKey = "RunScore";
    private const string ResultTimeKey = "RunTime";
    private const string ResultKillsKey = "RunKills";

    public static GameMode SelectedMode => ReadMode(SelectedModeKey, GameMode.Timed);
    public static bool HasResult => PlayerPrefs.GetInt(HasResultKey, 0) == 1;
    public static GameMode ResultMode => ReadMode(ResultModeKey, GameMode.Timed);
    public static bool ResultWon => PlayerPrefs.GetInt(ResultWonKey, 0) == 1;
    public static int ResultScore => PlayerPrefs.GetInt(ResultScoreKey, 0);
    public static float ResultTime => PlayerPrefs.GetFloat(ResultTimeKey, 0f);
    public static int ResultKills => PlayerPrefs.GetInt(ResultKillsKey, 0);

    public static void PrepareRun(GameMode mode)
    {
        PlayerPrefs.SetInt(SelectedModeKey, (int)mode);
        PlayerPrefs.SetInt(HasResultKey, 0);
        PlayerPrefs.Save();
    }

    public static void CompleteRun(GameMode mode, bool won, int score, float survivalTime, int kills)
    {
        PlayerPrefs.SetInt(HasResultKey, 1);
        PlayerPrefs.SetInt(ResultModeKey, (int)mode);
        PlayerPrefs.SetInt(ResultWonKey, won ? 1 : 0);
        PlayerPrefs.SetInt(ResultScoreKey, score);
        PlayerPrefs.SetFloat(ResultTimeKey, survivalTime);
        PlayerPrefs.SetInt(ResultKillsKey, kills);
        PlayerPrefs.Save();
    }

    public static void SaveEndlessRecord(string playerName)
    {
        if (ResultMode != GameMode.Endless || !HasResult)
            return;

        string cleanName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
        PlayerPrefs.SetString("LastEndlessName", cleanName);
        PlayerPrefs.SetInt("LastEndlessScore", ResultScore);
        PlayerPrefs.SetFloat("LastEndlessTime", ResultTime);
        PlayerPrefs.SetInt("LastEndlessKills", ResultKills);
        PlayerPrefs.Save();
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private static GameMode ReadMode(string key, GameMode fallback)
    {
        int value = PlayerPrefs.GetInt(key, (int)fallback);
        return value >= (int)GameMode.Menu && value <= (int)GameMode.Endless
            ? (GameMode)value
            : fallback;
    }
}
