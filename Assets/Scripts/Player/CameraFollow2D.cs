using UnityEngine;

// Giữ Camera đi theo người chơi với chuyển động mượt thay vì giật tức thời.
public class CameraFollow2D : MonoBehaviour
{
    // Mục tiêu cần theo, độ mượt và khoảng cách Camera so với mặt phẳng game.
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity;

    // LateUpdate chạy sau khi nhân vật đã di chuyển trong frame hiện tại.
    // Điều này giúp Camera theo nhân vật ổn định hơn.
    void LateUpdate()
    {
        if (target == null)
        {
            // Tự tìm Player nếu target chưa được gán trong Inspector.
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z;
        // SmoothDamp đưa Camera dần tới vị trí mong muốn trong smoothTime.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
