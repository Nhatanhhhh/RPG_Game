using UnityEngine;

public class ConfinerBoundary : MonoBehaviour
{
    [Header("Confiner Settings")]
    public PolygonCollider2D confinerCollider;

    [Header("Camera Settings")]
    public Camera mainCamera;

    [Header("Offset Settings")]
    [Tooltip("Distance from player to edge (0 = exact edge)")]
    public float edgeOffset;

    private float spriteHalfWidth;
    private float spriteHalfHeight;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteHalfWidth = sr.bounds.extents.x;
            spriteHalfHeight = sr.bounds.extents.y;
        }
    }

    /// <summary>
    /// Constrains player position within both camera viewport and polygon confiner bounds
    /// </summary>
    void LateUpdate()
    {
        if (confinerCollider == null || mainCamera == null) return;

        Vector3 pos = transform.position;

        // Calculate camera bounds
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        float cameraMinX = cameraPos.x - cameraWidth + spriteHalfWidth + edgeOffset;
        float cameraMaxX = cameraPos.x + cameraWidth - spriteHalfWidth - edgeOffset;
        float cameraMinY = cameraPos.y - cameraHeight + spriteHalfHeight + edgeOffset;
        float cameraMaxY = cameraPos.y + cameraHeight - spriteHalfHeight - edgeOffset;

        pos.x = Mathf.Clamp(pos.x, cameraMinX, cameraMaxX);
        pos.y = Mathf.Clamp(pos.y, cameraMinY, cameraMaxY);

        Vector2 testPos = new Vector2(pos.x, pos.y);

        if (!IsPointInPolygon(testPos))
        {
            pos = GetClosestPointInside(transform.position, pos);
        }

        transform.position = pos;
    }

    /// <summary>
    /// Checks if a point is inside the polygon collider
    /// </summary>
    bool IsPointInPolygon(Vector2 point)
    {
        return confinerCollider.OverlapPoint(point);
    }

    /// <summary>
    /// Uses binary search to find the closest valid point inside the polygon
    /// </summary>
    Vector3 GetClosestPointInside(Vector3 oldPos, Vector3 newPos)
    {
        Vector2 direction = (newPos - oldPos).normalized;
        float maxDistance = Vector3.Distance(oldPos, newPos);

        float step = maxDistance;
        Vector3 testPos = oldPos;

        for (int i = 0; i < 10; i++)
        {
            step *= 0.5f;
            Vector3 nextPos = testPos + (Vector3)direction * step;

            if (IsPointInPolygon(nextPos))
            {
                testPos = nextPos;
            }
        }

        return testPos;
    }

    /// <summary>
    /// Draws debug gizmos showing camera bounds and polygon confiner in Scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null || confinerCollider == null) return;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(cameraPos, new Vector3(cameraWidth * 2, cameraHeight * 2, 0));

        Gizmos.color = Color.yellow;
        Vector2[] points = confinerCollider.GetPath(0);
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 start = confinerCollider.transform.TransformPoint(points[i]);
            Vector2 end = confinerCollider.transform.TransformPoint(points[(i + 1) % points.Length]);
            Gizmos.DrawLine(start, end);
        }
    }
}