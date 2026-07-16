using UnityEngine;
using UnityEngine.SceneManagement; // BẮT BUỘC để chuyển Scene
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên của Scene Menu chính

    private bool isPaused = false;

    void Start()
    {
        // Ẩn panel khi vào game cho chắc chắn
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Lắng nghe sự kiện click chuột của các nút
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => TogglePause(false));

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    void Update()
    {
        // Kiểm tra nếu người chơi bấm nút Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Đảo trạng thái Pause hiện tại
            TogglePause(!isPaused);
        }
    }

    public void TogglePause(bool pause)
    {
        isPaused = pause;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        // Dừng hoặc tiếp tục thời gian của game
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void LoadMainMenu()
    {
        // BẮT BUỘC: Trả thời gian game về bình thường trước khi chuyển cảnh
        Time.timeScale = 1f; 
        
        // Chuyển về Scene Main Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}