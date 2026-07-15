using UnityEngine;

[DisallowMultipleComponent]
public class MapBounds2D : MonoBehaviour
{
    public Vector2 center = Vector2.zero;
    public Vector2 size = new Vector2(30f, 30f);
    [Min(0f)] public float playerPadding = 1.2f;
    [Min(0f)] public float enemyPadding = 0.6f;

    public Bounds WorldBounds
    {
        get
        {
            Vector2 worldCenter = (Vector2)transform.position + center;
            return new Bounds(worldCenter, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 0f));
        }
    }

    public Vector2 ClampPoint(Vector2 point, float padding)
    {
        Bounds bounds = WorldBounds;
        float paddingX = Mathf.Min(Mathf.Max(0f, padding), bounds.extents.x);
        float paddingY = Mathf.Min(Mathf.Max(0f, padding), bounds.extents.y);

        return new Vector2(
            Mathf.Clamp(point.x, bounds.min.x + paddingX, bounds.max.x - paddingX),
            Mathf.Clamp(point.y, bounds.min.y + paddingY, bounds.max.y - paddingY));
    }

    public Vector3 ClampCameraPosition(Vector3 position, Camera targetCamera)
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return position;

        Bounds bounds = WorldBounds;
        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        position.x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : bounds.center.x;
        position.y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : bounds.center.y;
        return position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.15f, 0.9f, 1f, 0.9f);
        Bounds bounds = WorldBounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
