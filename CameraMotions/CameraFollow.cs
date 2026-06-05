using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    public Transform player;
    public float smoothSpeed = 5f;
    public float verticalOffset = 4f;

    [Header("Zoom")]
    public Camera cam;
    public float normalSize = 10f;
    public float zoomSpeed = 4f;

    private float targetSize;

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
        {
            normalSize = cam.orthographicSize;
            targetSize = normalSize;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y + verticalOffset,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetSize,
                zoomSpeed * Time.deltaTime
            );
        }
    }

    public void ZoomOutTemporary()
    {
        targetSize = normalSize + 2f;
    }

    public void ZoomOutTemporary(float zoomSize)
    {
        targetSize = zoomSize;
    }

    public void ZoomBackIn()
    {
        targetSize = normalSize;
    }

    public void SetZoom(float newSize)
    {
        targetSize = newSize;
    }

    public float GetNormalSize()
    {
        return normalSize;
    }
}
