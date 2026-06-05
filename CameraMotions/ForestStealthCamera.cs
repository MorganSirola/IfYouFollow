using UnityEngine;

public class ForestStealthCamera : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform player;

    [Header("Scene Bounds / Progress")]
    public Transform sceneStartMarker;
    public Transform sceneEndMarker;

    [Header("Position Framing")]
    public Transform wideViewCenter;
    public Transform closeViewCenter;
    public float moveSmoothSpeed = 4f;

    [Header("Zoom")]
    public float wideZoomSize = 18f;
    public float closeZoomSize = 10f;
    public float zoomSmoothSpeed = 4f;

    [Header("State")]
    public bool cameraActive = false;

    private float targetZoom;
    private Vector3 targetPosition;
    public CameraFollow normalCamera;

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
            targetZoom = cam.orthographicSize;

        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!cameraActive || cam == null || player == null)
            return;

        float progress = GetSceneProgress();

        if (wideViewCenter != null && closeViewCenter != null)
        {
            targetPosition = Vector3.Lerp(
                wideViewCenter.position,
                closeViewCenter.position,
                progress
            );

            targetPosition.z = -10f;
        }

        targetZoom = Mathf.Lerp(wideZoomSize, closeZoomSize, progress);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSmoothSpeed * Time.deltaTime
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSmoothSpeed * Time.deltaTime
        );
    }

    float GetSceneProgress()
    {
        if (sceneStartMarker == null || sceneEndMarker == null || player == null)
            return 0f;

        float startX = sceneStartMarker.position.x;
        float endX = sceneEndMarker.position.x;
        float playerX = player.position.x;

        if (Mathf.Abs(endX - startX) < 0.001f)
            return 0f;

        return Mathf.Clamp01(Mathf.InverseLerp(startX, endX, playerX));
    }

    public void ActivateStealthCamera()
    {
        cameraActive = true;

        if (normalCamera != null)
            normalCamera.enabled = false;

        if (wideViewCenter != null)
        {
            Vector3 startPos = wideViewCenter.position;
            startPos.z = -10f;
            transform.position = startPos;
        }

        if (cam != null)
            cam.orthographicSize = wideZoomSize;
    }

    public void DeactivateStealthCamera()
        {
            cameraActive = false;

            if (normalCamera != null)
                normalCamera.enabled = true;
        }
}