using UnityEngine;

public static class PlayerRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Camera mainCamera = Camera.main;

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
