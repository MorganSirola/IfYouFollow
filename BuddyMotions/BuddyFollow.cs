using System.Collections.Generic;
using UnityEngine;

public class BuddyFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerMovements playerMovements;

    [Header("Follow Feel")]
    public float followDelaySeconds = 0.25f;  // how far behind
    public float catchUpSpeed = 8f; //how fast buddy moves to player
    public float maxDistanceBeforeSnap = 4f; // incase he gets stuck

    [Header("Offset")]
    public float followDistanceX = 1.2f;
    public float followOffsetY = 0f;

    [Header("Jump")]
    public float jumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Crouch")]
    public Vector3 crouchScale = new Vector3(1f, 0.7f, 1f);

    [Header("Animation")]
    public Animator anim;
    public SpriteRenderer sr;

    [Header("Trust Demo")]
    [Range(0f, 100f)] public float trust = 50f;
    public float trustGainPerSecond = 12f;
    public float trustLossPerSecond = 6f;

    public float highTrustThreshold = 70f;
    public float mediumTrustThreshold = 40f; // for demo, below this = low trust

    [Header("Trust Speeds")]
    public float mediumWalkMaxSpeed = 3.5f;
    public float mediumSprintMaxSpeed = 5f;
    public float highMaxSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 lastRbPos;
    private Vector2 targetPos;
    private Vector3 normalScale;
    private bool isGrounded;

    public float stopDistance = 0.08f;

    [Header("Intro Overrides")]
    public bool allowCrouchMimic = true;
    public bool allowJumpMimic = true;
    public bool allowSprintBoost = true;

    private struct Point
    {
        public Vector2 pos;
        public float time;
        public bool crouching;
        public float jumpTime;
        public bool facingLeft;

        public Point(Vector2 p, float t, bool c, float jt, bool f) 
        {
            pos = p; 
            time = t;
            crouching = c;
            jumpTime = jt;
            facingLeft = f;
        }
    }

    private readonly List<Point> history = new List<Point>();
    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        normalScale = transform.localScale;

        //smooth motion
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            lastRbPos = rb.position;
        }
    }

    void OnEnable()
    {
        if (rb != null)
            lastRbPos = rb.position;
    }


    void Update()
    {
        if (player == null || rb == null) return;

        //buddy ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else   
            isGrounded = true;

        //get player facing direction
        bool playerFacingLeft = false;
        SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR != null)
            playerFacingLeft = playerSR.flipX;


        //record players state over time
        history.Add(new Point(
            player.position, 
            Time.time,
            playerMovements.IsCrouching,
            playerMovements.LastJumpTime,
            playerFacingLeft
        ));

        //remove old points
        float cutoff = Time.time - (followDelaySeconds + 1f);
        while (history.Count > 0 && history[0].time < cutoff)
            history.RemoveAt(0);

        float targetTime = Time.time - followDelaySeconds;

        //fallback point
        Point delayedPoint = new Point(
            player.position,
            Time.time,
            false,
            -999f,
            playerFacingLeft
        );

        //find delayed position
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].time >= targetTime)
            {
                delayedPoint = history[i];
                break;
            }
        }

        bool highTrust = trust >= highTrustThreshold;
        bool mediumTrust = trust >= mediumTrustThreshold;

        //stay behind player based on facing direction
        float dir = delayedPoint.facingLeft ? -1f : 1f;
        Vector2 offset = new Vector2(-dir * followDistanceX, followOffsetY);
        targetPos = delayedPoint.pos + offset;

        //facing movement direction
        float vxToTarget = targetPos.x - rb.position.x;
        if (sr != null)
        {
            if (vxToTarget < -0.001f) sr.flipX = true;
            else if (vxToTarget > 0.001f) sr.flipX = false;
        }

        // LOW TRUST: stop all together
        if (!mediumTrust)
        {
            transform.localScale = normalScale;

            if (anim != null)
                anim.SetBool("isWalking", false);

            return;
        }

        // HIGH TRUST: crouch mimic
        transform.localScale = (allowCrouchMimic && highTrust && delayedPoint.crouching)
            ? new Vector3(normalScale.x, normalScale.y * crouchScale.y, normalScale.z)
            : normalScale;

        // HIGH TRUST: jump mimic
        if (allowJumpMimic && highTrust && isGrounded && Mathf.Abs(delayedPoint.time - delayedPoint.jumpTime) < 0.04f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null || playerMovements == null) return;

        //snap if too far
        if (Vector2.Distance(rb.position, targetPos) > maxDistanceBeforeSnap)
        {
            rb.position = targetPos;
            lastRbPos = rb.position;
            
            if (anim != null)
                anim.SetBool("isWalking", false);
            
            return;
        }

        bool highTrust = trust >= highTrustThreshold;
        bool mediumTrust = trust >= mediumTrustThreshold;

        // LOW TRUST: stop completely
        if (!mediumTrust)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (anim != null)
                anim.SetBool("isWalking", false);

            return;
        }

        float maxXSpeed;

        if (highTrust)
        {
            // high trust = keeps up closely with player
            maxXSpeed = highMaxSpeed;
        }
        else
        {
            // medium trust = normal pace, but can speed up if player sprints
            bool playerSprinting = allowSprintBoost && playerMovements.IsSprinting;            
            maxXSpeed = playerSprinting ? mediumSprintMaxSpeed : mediumWalkMaxSpeed;
        }

        // move using velocity on X only, let physics handle Y
        float xDiff = targetPos.x - rb.position.x;

        // if close enough, stop completely
        if (Mathf.Abs(xDiff) <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (anim != null)
                anim.SetBool("isWalking", false);

            lastRbPos = rb.position;
            return;
        }

        float xVelocity = xDiff * catchUpSpeed;

        // clamp speed based on trust level
        xVelocity = Mathf.Clamp(xVelocity, -maxXSpeed, maxXSpeed);

        rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);

        // walk detection
        bool walking = Mathf.Abs(xVelocity) > 0.05f;

        if (anim != null)
            anim.SetBool("isWalking", walking);

        lastRbPos = rb.position;
    }

    public void TickTrust(bool holdingHands, float dt)
    {
        if (holdingHands)
            trust = Mathf.Min(100f, trust + trustGainPerSecond * dt);
        else
            trust = Mathf.Max(0f, trust - trustLossPerSecond * dt);
    }

    public void ForceNormalPose()
    {
        transform.localScale = normalScale;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.speed = 1f;
        }

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

}


