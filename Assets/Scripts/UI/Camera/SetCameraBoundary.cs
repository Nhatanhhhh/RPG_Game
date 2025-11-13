using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SetCameraBoundary : MonoBehaviour
{
    private PolygonCollider2D mapBoundary;

    void Awake()
    {
        mapBoundary = GetComponent<PolygonCollider2D>();
    }

    void Start()
    {
        CinemachineConfiner cameraConfiner = FindObjectOfType<CinemachineConfiner>();

        if (cameraConfiner != null)
        {
            cameraConfiner.m_BoundingShape2D = mapBoundary;

            cameraConfiner.InvalidatePathCache();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CinemachineConfiner trong scene!");
        }
    }
}