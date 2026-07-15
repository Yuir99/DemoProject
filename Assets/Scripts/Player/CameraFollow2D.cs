using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity;
    private Camera targetCamera;
    private MapBounds2D mapBounds;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        mapBounds = FindFirstObjectByType<MapBounds2D>();
    }

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

        mapBounds ??= FindFirstObjectByType<MapBounds2D>();
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z;

        if (mapBounds != null)
            desiredPosition = mapBounds.ClampCameraPosition(desiredPosition, targetCamera);

        Vector3 nextPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime);

        if (mapBounds != null)
            nextPosition = mapBounds.ClampCameraPosition(nextPosition, targetCamera);

        transform.position = nextPosition;
    }
}
