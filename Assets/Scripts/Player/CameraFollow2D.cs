using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
