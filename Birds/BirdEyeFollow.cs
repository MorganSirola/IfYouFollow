using UnityEngine;

public class BirdEyeFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform leftPupil;
    public Transform rightPupil;

    [Header("Eye Movement")]
    public float maxXOffset = 0.06f;
    public float maxYOffset = 0.04f;
    public float smoothSpeed = 8f;

    [Header("Optional Distance Limit")]
    public bool useDistanceLimit = false;
    public float maxTrackDistance = 10f;

    private Vector3 leftStartLocalPos;
    private Vector3 rightStartLocalPos;

    void Start()
    {
        if (leftPupil != null)
            leftStartLocalPos = leftPupil.localPosition;

        if (rightPupil != null)
            rightStartLocalPos = rightPupil.localPosition;
    }

    void Update()
    {
        if (player == null || leftPupil == null || rightPupil == null)
            return;

        if (useDistanceLimit)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > maxTrackDistance)
            {
                ReturnEyesToCenter();
                return;
            }
        }

        Vector3 directionToPlayer = player.position - transform.position;

        // Normalize so the movement stays between -1 and 1
        directionToPlayer.Normalize();

        Vector3 leftTarget = leftStartLocalPos + new Vector3(
            directionToPlayer.x * maxXOffset,
            directionToPlayer.y * maxYOffset,
            0f
        );

        Vector3 rightTarget = rightStartLocalPos + new Vector3(
            directionToPlayer.x * maxXOffset,
            directionToPlayer.y * maxYOffset,
            0f
        );

        leftPupil.localPosition = Vector3.Lerp(
            leftPupil.localPosition,
            leftTarget,
            smoothSpeed * Time.deltaTime
        );

        rightPupil.localPosition = Vector3.Lerp(
            rightPupil.localPosition,
            rightTarget,
            smoothSpeed * Time.deltaTime
        );
    }

    void ReturnEyesToCenter()
    {
        leftPupil.localPosition = Vector3.Lerp(
            leftPupil.localPosition,
            leftStartLocalPos,
            smoothSpeed * Time.deltaTime
        );

        rightPupil.localPosition = Vector3.Lerp(
            rightPupil.localPosition,
            rightStartLocalPos,
            smoothSpeed * Time.deltaTime
        );
    }
}