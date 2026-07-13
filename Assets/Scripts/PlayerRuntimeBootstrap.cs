using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateBootstrapper()
    {
        if (Object.FindFirstObjectByType<RuntimeSceneBootstrapper>() != null)
            return;

        GameObject bootstrapper = new GameObject("Runtime Scene Bootstrapper");
        Object.DontDestroyOnLoad(bootstrapper);
        bootstrapper.AddComponent<RuntimeSceneBootstrapper>();
    }
}

public class RuntimeSceneBootstrapper : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        SetupScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupScene();
    }

    void SetupScene()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Camera mainCamera = Camera.main;

        if (Object.FindFirstObjectByType<GameFlowManager>() == null)
        {
            GameObject flow = new GameObject("Game Flow Manager");
            flow.AddComponent<GameFlowManager>();
        }

        if (mainCamera != null)
        {
            CameraFollow2D follow = mainCamera.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = mainCamera.gameObject.AddComponent<CameraFollow2D>();

            if (player != null)
                follow.target = player.transform;
        }

        if (Object.FindFirstObjectByType<GameHUD>() == null)
        {
            GameObject hud = new GameObject("Runtime HUD");
            hud.AddComponent<GameHUD>();
        }

        if (Object.FindFirstObjectByType<LevelUpUpgradeUI>() == null)
        {
            GameObject upgradeUI = new GameObject("Level Up Upgrade UI");
            upgradeUI.AddComponent<LevelUpUpgradeUI>();
        }
    }
}
