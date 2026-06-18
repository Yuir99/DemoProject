using UnityEngine;

// Tự động chuẩn bị các hệ thống cần thiết sau khi Scene được tải.
// Nhờ script này, Camera và UI không cần được tạo thủ công trong Hierarchy.
public static class PlayerRuntimeBootstrap
{
    // Unity tự gọi Setup sau mỗi lần tải Scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Gắn CameraFollow2D nếu Camera chưa có script này.
            CameraFollow2D follow = mainCamera.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = mainCamera.gameObject.AddComponent<CameraFollow2D>();

            if (player != null)
                follow.target = player.transform;
        }

        if (Object.FindFirstObjectByType<GameHUD>() == null)
        {
            // Tạo HUD máu, XP, linh hồn, Lõi và thời gian.
            GameObject hud = new GameObject("Runtime HUD");
            hud.AddComponent<GameHUD>();
        }

        if (Object.FindFirstObjectByType<LevelUpUpgradeUI>() == null)
        {
            // Tạo bảng chọn nâng cấp khi người chơi lên cấp.
            GameObject upgradeUI = new GameObject("Level Up Upgrade UI");
            upgradeUI.AddComponent<LevelUpUpgradeUI>();
        }
    }
}
