using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public static GameMode PendingMode { get; private set; } = GameMode.Menu;

    private const string RequestedModeKey = "RequestedGameMode";

    public GameMode CurrentMode { get; private set; } = GameMode.Menu;
    public bool GameIsRunning { get; private set; }
    public bool GameHasEnded { get; private set; }
    public int EnemyKills { get; private set; }
    public int EndlessScore { get; private set; }
    public float SurvivalTime { get; private set; }

    private Canvas canvas;
    private GameObject menuPanel;
    private GameObject resultPanel;
    private Text lastRunText;
    private InputField nameInput;
    private WaveSpawner waveSpawner;

    public static bool IsEndlessMode => Instance != null && Instance.CurrentMode == GameMode.Endless;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureEventSystem();
        BuildCanvas();
    }

    void Start()
    {
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        PendingMode = GetRequestedMode();
        SetRequestedMode(GameMode.Menu);

        if (PendingMode == GameMode.Menu)
            ShowMenu();
        else
            StartMode(PendingMode);
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
        PendingMode = mode;
        GameIsRunning = true;
        GameHasEnded = false;
        EnemyKills = 0;
        EndlessScore = 0;
        SurvivalTime = 0f;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        Time.timeScale = 1f;

        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

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
        if (CurrentMode != GameMode.Timed)
            return;

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

        if (CurrentMode == GameMode.Endless)
            EndlessScore = CalculateEndlessScore();

        Time.timeScale = 0f;
        ShowResult(won);
    }

    void ShowMenu()
    {
        CurrentMode = GameMode.Menu;
        PendingMode = GameMode.Menu;
        SetRequestedMode(GameMode.Menu);
        GameIsRunning = false;
        GameHasEnded = false;
        Time.timeScale = 0f;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (menuPanel == null)
            BuildMenu();

        RefreshLastRunText();
        menuPanel.SetActive(true);
    }

    void ShowResult(bool won)
    {
        if (resultPanel != null)
            Destroy(resultPanel);

        resultPanel = CreateFullscreenPanel("Result Panel", new Color(0.01f, 0.015f, 0.025f, 0.92f));
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        string title = won ? "You Win - The Core Still Burns" : "You Lose - The Core Has Fallen";
        if (CurrentMode == GameMode.Endless)
            title = "Endless Run Ended";

        Text titleText = CreateText(resultPanel.transform, font, title, 42, new Vector2(0f, 170f), new Vector2(900f, 70f));
        titleText.alignment = TextAnchor.MiddleCenter;

        string details = CurrentMode == GameMode.Endless
            ? $"Score {EndlessScore}\nSurvived {FormatTime(SurvivalTime)}\nKills {EnemyKills}"
            : won
                ? $"You protected the Energy Core for {FormatTime(waveSpawner == null ? SurvivalTime : waveSpawner.matchDuration)}."
                : $"Survived {FormatTime(SurvivalTime)} before the Core collapsed.";

        Text detailText = CreateText(resultPanel.transform, font, details, 26, new Vector2(0f, 80f), new Vector2(760f, 110f));
        detailText.alignment = TextAnchor.MiddleCenter;

        if (CurrentMode == GameMode.Endless)
            BuildEndlessNameEntry(resultPanel.transform, font);

        Button retryButton = CreateButton(resultPanel.transform, font, "Retry", new Vector2(-150f, -180f), new Vector2(220f, 58f));
        retryButton.onClick.AddListener(RetryCurrentMode);

        Button menuButton = CreateButton(resultPanel.transform, font, "Back To Menu", new Vector2(150f, -180f), new Vector2(220f, 58f));
        menuButton.onClick.AddListener(ReturnToMenu);

        resultPanel.SetActive(true);
    }

    void BuildEndlessNameEntry(Transform parent, Font font)
    {
        Text label = CreateText(parent, font, "Record your name for this run", 22, new Vector2(0f, -30f), new Vector2(520f, 36f));
        label.alignment = TextAnchor.MiddleCenter;

        GameObject inputObject = new GameObject("Name Input");
        inputObject.transform.SetParent(parent, false);
        RectTransform inputRect = inputObject.AddComponent<RectTransform>();
        inputRect.anchorMin = inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(-85f, -82f);
        inputRect.sizeDelta = new Vector2(330f, 52f);

        Image image = inputObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.1f, 0.16f, 1f);

        nameInput = inputObject.AddComponent<InputField>();
        nameInput.characterLimit = 16;

        Text text = CreateText(inputObject.transform, font, "", 22, Vector2.zero, new Vector2(300f, 44f));
        text.alignment = TextAnchor.MiddleLeft;
        text.rectTransform.anchoredPosition = new Vector2(8f, 0f);
        nameInput.textComponent = text;

        Text placeholder = CreateText(inputObject.transform, font, "Name", 22, Vector2.zero, new Vector2(300f, 44f));
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = new Color(1f, 1f, 1f, 0.35f);
        placeholder.rectTransform.anchoredPosition = new Vector2(8f, 0f);
        nameInput.placeholder = placeholder;
        nameInput.text = PlayerPrefs.GetString("LastEndlessName", "Player");

        Button saveButton = CreateButton(parent, font, "Save", new Vector2(160f, -82f), new Vector2(140f, 52f));
        saveButton.onClick.AddListener(SaveEndlessRecord);
    }

    void SaveEndlessRecord()
    {
        string playerName = nameInput == null ? "Player" : nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";

        PlayerPrefs.SetString("LastEndlessName", playerName);
        PlayerPrefs.SetInt("LastEndlessScore", EndlessScore);
        PlayerPrefs.SetFloat("LastEndlessTime", SurvivalTime);
        PlayerPrefs.SetInt("LastEndlessKills", EnemyKills);
        PlayerPrefs.Save();
    }

    void RetryCurrentMode()
    {
        if (CurrentMode == GameMode.Endless)
            SaveEndlessRecord();

        PendingMode = CurrentMode == GameMode.Endless ? GameMode.Endless : GameMode.Timed;
        SetRequestedMode(PendingMode);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ReturnToMenu()
    {
        if (CurrentMode == GameMode.Endless)
            SaveEndlessRecord();

        PendingMode = GameMode.Menu;
        SetRequestedMode(GameMode.Menu);
        ShowMenu();
    }

    void LoadModeFromMenu(GameMode mode)
    {
        PendingMode = mode;
        SetRequestedMode(mode);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    static void SetRequestedMode(GameMode mode)
    {
        PlayerPrefs.SetInt(RequestedModeKey, (int)mode);
        PlayerPrefs.Save();
    }

    static GameMode GetRequestedMode()
    {
        int value = PlayerPrefs.GetInt(RequestedModeKey, (int)GameMode.Menu);
        if (value < (int)GameMode.Menu || value > (int)GameMode.Endless)
            return GameMode.Menu;

        return (GameMode)value;
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    int CalculateEndlessScore()
    {
        int wave = waveSpawner == null ? 0 : waveSpawner.currentWave;
        return Mathf.FloorToInt(SurvivalTime * 10f) + EnemyKills * 75 + wave * 150;
    }

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Game Flow UI");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    void BuildMenu()
    {
        menuPanel = CreateFullscreenPanel("Main Menu", new Color(0.01f, 0.015f, 0.025f, 0.94f));
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Text title = CreateText(menuPanel.transform, font, "Soul Core Siege", 58, new Vector2(0f, 210f), new Vector2(900f, 88f));
        title.alignment = TextAnchor.MiddleCenter;

        Text subtitle = CreateText(menuPanel.transform, font, "Hold the line. Feed the turrets. Keep the core alive.", 24, new Vector2(0f, 150f), new Vector2(900f, 48f));
        subtitle.alignment = TextAnchor.MiddleCenter;
        subtitle.color = new Color(0.78f, 0.86f, 1f, 1f);

        Button timedButton = CreateButton(menuPanel.transform, font, "Timed Defense", new Vector2(0f, 40f), new Vector2(360f, 64f));
        timedButton.onClick.AddListener(() => LoadModeFromMenu(GameMode.Timed));

        Button endlessButton = CreateButton(menuPanel.transform, font, "Endless Stand", new Vector2(0f, -40f), new Vector2(360f, 64f));
        endlessButton.onClick.AddListener(() => LoadModeFromMenu(GameMode.Endless));

        Button quitButton = CreateButton(menuPanel.transform, font, "Quit Game", new Vector2(0f, -120f), new Vector2(360f, 64f));
        quitButton.onClick.AddListener(QuitGame);

        lastRunText = CreateText(menuPanel.transform, font, "", 20, new Vector2(620f, -390f), new Vector2(560f, 120f));
        lastRunText.alignment = TextAnchor.LowerRight;
        lastRunText.color = new Color(0.9f, 0.95f, 1f, 0.9f);
    }

    GameObject CreateFullscreenPanel(string objectName, Color color)
    {
        GameObject panel = new GameObject(objectName);
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    Text CreateText(Transform parent, Font font, string value, int fontSize, Vector2 position, Vector2 size)
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
        text.text = value;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        return text;
    }

    Button CreateButton(Transform parent, Font font, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(label);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.1f, 0.15f, 0.22f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.2f, 0.34f, 0.45f, 1f);
        colors.pressedColor = new Color(0.12f, 0.55f, 0.5f, 1f);
        button.colors = colors;

        Text text = CreateText(buttonObject.transform, font, label, 24, Vector2.zero, new Vector2(size.x - 24f, size.y - 8f));
        text.alignment = TextAnchor.MiddleCenter;

        return button;
    }

    void RefreshLastRunText()
    {
        if (lastRunText == null)
            return;

        int score = PlayerPrefs.GetInt("LastEndlessScore", 0);
        if (score <= 0)
        {
            lastRunText.text = "No endless record yet.";
            return;
        }

        string playerName = PlayerPrefs.GetString("LastEndlessName", "Player");
        float seconds = PlayerPrefs.GetFloat("LastEndlessTime", 0f);
        int kills = PlayerPrefs.GetInt("LastEndlessKills", 0);
        lastRunText.text = $"Last Endless Run\n{playerName}  Score {score}\nSurvived {FormatTime(seconds)}  Kills {kills}";
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{minutes:00}:{secs:00}";
    }
}

public enum GameMode { Menu, Timed, Endless }
