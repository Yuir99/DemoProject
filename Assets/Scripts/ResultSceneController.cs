using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultSceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultDetails;
    [SerializeField] private GameObject endlessNameGroup;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    void Awake()
    {
        Time.timeScale = 1f;
        BindSceneUI();
        ConfigureResult();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveRecord);
        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
        if (menuButton != null)
            menuButton.onClick.AddListener(BackToMenu);
    }

    void BindSceneUI()
    {
        resultTitle ??= SceneObjectLookup.FindComponent<TMP_Text>("ResultTitle");
        resultDetails ??= SceneObjectLookup.FindComponent<TMP_Text>("ResultDetails");
        endlessNameGroup ??= SceneObjectLookup.FindGameObject("EndlessNameGroup");
        nameInput ??= SceneObjectLookup.FindComponent<TMP_InputField>("NameInput");
        saveButton ??= SceneObjectLookup.FindComponent<Button>("SaveButton");
        retryButton ??= SceneObjectLookup.FindComponent<Button>("RetryButton");
        menuButton ??= SceneObjectLookup.FindComponent<Button>("MenuButton");
    }

    void ConfigureResult()
    {
        if (!GameSession.HasResult)
        {
            if (resultTitle != null)
                resultTitle.text = "NO RUN RESULT";
            if (resultDetails != null)
                resultDetails.text = "Start a match from the main menu.";
            if (endlessNameGroup != null)
                endlessNameGroup.SetActive(false);
            return;
        }

        bool endless = GameSession.ResultMode == GameMode.Endless;
        if (endlessNameGroup != null)
            endlessNameGroup.SetActive(endless);

        if (nameInput != null)
            nameInput.text = PlayerPrefs.GetString("LastEndlessName", "Player");

        if (resultTitle != null)
        {
            resultTitle.text = endless
                ? "ENDLESS STAND ENDED"
                : GameSession.ResultWon ? "THE CORE SURVIVES" : "THE CORE HAS FALLEN";
        }

        if (resultDetails == null)
            return;

        resultDetails.text = endless
            ? $"SCORE {GameSession.ResultScore}\nSURVIVED {GameSession.FormatTime(GameSession.ResultTime)}\nKILLS {GameSession.ResultKills}"
            : GameSession.ResultWon
                ? $"The Energy Core survived for {GameSession.FormatTime(GameSession.ResultTime)}."
                : $"The Core fell after {GameSession.FormatTime(GameSession.ResultTime)}.";
    }

    public void SaveRecord()
    {
        string playerName = nameInput == null ? "Player" : nameInput.text;
        GameSession.SaveEndlessRecord(playerName);

        if (saveButton != null)
        {
            TMP_Text label = saveButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = "SAVED";
        }
    }

    public void Retry()
    {
        SaveEndlessIfNeeded();
        GameSession.PrepareRun(GameSession.ResultMode);
        SceneManager.LoadScene("GameScene");
    }

    public void BackToMenu()
    {
        SaveEndlessIfNeeded();
        SceneManager.LoadScene("MainMenu");
    }

    void SaveEndlessIfNeeded()
    {
        if (GameSession.HasResult && GameSession.ResultMode == GameMode.Endless)
            SaveRecord();
    }
}
