using UnityEngine;
using UnityEngine.InputSystem;

public class BuddyHandHold : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public BuddyFollow buddyFollow;
    public PlayerMovements playerMovements;

    [Header("Optional Visual References")]
    public Animator buddyAnim;
    public Animator playerAnim;
    public SpriteRenderer buddySR;
    public SpriteRenderer playerSR;

    [Header("Attach Settings")]
    public Vector2 holdOffsetRight = new Vector2(-0.6f, 0f);
    public Vector2 holdOffsetLeft = new Vector2(0.6f, 0f);
    public float latchDistance = 1.2f;
    public float holdMoveSpeed = 10f;

    [Header("Controls")]
    public Key holdKey = Key.E;

    [Header("Rules")]
    public bool allowHandHold = true;
    public bool requireCloseDistance = true;
    public bool toggleMode = true;

    [Header("Hand Anchor Points")]
    public Transform playerHandRight;
    public Transform playerHandLeft;
    public Transform buddyHandRight;
    public Transform buddyHandLeft;

    private bool isHoldingHands = false;
    private Rigidbody2D rb;
    private Transform originalParent;

    public bool IsHoldingHands => isHoldingHands;

    public bool CanLatch()
    {
        float dist = Vector2.Distance(player.position, transform.position);
        return dist <= latchDistance;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (buddyAnim == null) buddyAnim = GetComponent<Animator>();
        if (buddySR == null) buddySR = GetComponent<SpriteRenderer>();

        if (player != null)
        {
            if (playerAnim == null) playerAnim = player.GetComponent<Animator>();
            if (playerSR == null) playerSR = player.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (player == null) return;
        if (Keyboard.current == null) return;

        bool pressed = Keyboard.current[holdKey].wasPressedThisFrame;
        bool released = Keyboard.current[holdKey].wasReleasedThisFrame;

        float dist = Vector2.Distance(transform.position, player.position);
        bool closeEnough = dist <= latchDistance;

        // can only actually latch if allowed + close enough (if required)
        bool canActuallyLatch = allowHandHold && (!requireCloseDistance || closeEnough);

        if (pressed)
        {
            Debug.Log("Hold key detected");

            if (!isHoldingHands)
            {
                if (canActuallyLatch)
                {
                    StartHoldingHands();
                }
                else
                {
                    Debug.Log("Too far / not allowed -> reach hand instead");

                    if (playerMovements != null)
                        playerMovements.SetReachingHand(true);

                    if (playerAnim != null)
                        playerAnim.SetBool("isReachingHand", true);
                }
            }
            else if (toggleMode)
            {
                StopHoldingHands();
            }
        }

        if (released && !isHoldingHands)
        {
            if (playerMovements != null)
                playerMovements.SetReachingHand(false);

            if (playerAnim != null)
                playerAnim.SetBool("isReachingHand", false);
        }
    }

    void FixedUpdate()
    {
        if (isHoldingHands)
        {
            UpdateHoldPosition();
        }
    }

    void StartHoldingHands()
    {
        Debug.Log("Started holding hands");
        isHoldingHands = true;

        originalParent = transform.parent;

        if (buddyFollow != null)
            buddyFollow.enabled = false;

        if (playerMovements != null)
        {
            playerMovements.SetReachingHand(false);
            playerMovements.SetHandHolding(true);
        }

        if (playerAnim != null)
        {
            playerAnim.SetBool("isReachingHand", false);
            playerAnim.SetBool("isHoldingHands", true);
        }

        if (buddyAnim != null)
        {
            buddyAnim.SetBool("isHoldingHands", true);
            buddyAnim.SetBool("isWalking", false);
        }
    }

    void StopHoldingHands()
    {
        isHoldingHands = false;

        if (buddyFollow != null)
            buddyFollow.enabled = true;

        if (playerMovements != null)
        {
            playerMovements.SetHandHolding(false);
            playerMovements.SetReachingHand(false);
        }

        if (buddyAnim != null)
        {
            buddyAnim.SetBool("isHoldingHands", false);
            buddyAnim.SetBool("isWalking", false);
        }

        if (playerAnim != null)
        {
            playerAnim.SetBool("isHoldingHands", false);
            playerAnim.SetBool("isReachingHand", false);
        }
    }

    void UpdateHoldPosition()
    {
        if (player == null) return;

        bool playerFacingLeft = false;
        if (playerSR != null)
            playerFacingLeft = playerSR.flipX;

        if (buddySR != null)
            buddySR.flipX = playerFacingLeft;

        // choose the correct hand markers
        Transform playerHand = playerFacingLeft ? playerHandLeft : playerHandRight;
        Transform buddyHand = playerFacingLeft ? buddyHandLeft : buddyHandRight;

        if (playerHand == null || buddyHand == null) return;

        // keep buddy walk/idle matching player while holding hands
        bool playerWalking = false;
        if (playerAnim != null)
            playerWalking = playerAnim.GetBool("isWalking");

        if (buddyAnim != null)
            buddyAnim.SetBool("isWalking", playerWalking);

        // move buddy so buddy hand lines up with player hand
        Vector3 offsetToBuddyRoot = transform.position - buddyHand.position;
        transform.position = playerHand.position + offsetToBuddyRoot;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void ForceStopHoldingHands()
    {
        if (isHoldingHands)
            StopHoldingHands();
    }
}
