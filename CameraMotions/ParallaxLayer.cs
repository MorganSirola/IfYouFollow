using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Parallax")]
    [Range(0f, 2f)]
    public float parallaxMultiplier = 0.5f;

    [Header("Axis Control")]
    public bool moveX = true;
    public bool moveY = false;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        startPosition = transform.position;

        if (cameraTransform != null)
            cameraStartPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 cameraOffset = cameraTransform.position - cameraStartPosition;

        float x = startPosition.x;
        float y = startPosition.y;

        if (moveX)
            x += cameraOffset.x * parallaxMultiplier;

        if (moveY)
            y += cameraOffset.y * parallaxMultiplier;

        transform.position = new Vector3(x, y, startPosition.z);
    }
}