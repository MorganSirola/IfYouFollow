using UnityEngine;
using UnityEngine.InputSystem;

public class HoldPairController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform buddy;
    public BuddyFollow buddyFollow;
    public PlayerMovements playerMovements;

    [Header("Real Character Visuals")]
    public SpriteRenderer playerSR;
    public SpriteRenderer buddySR;
    public Animator playerAnim;
    public Animator buddyAnim;

    [Header("Hold Pair")]
    public GameObject holdPair;
    public Animator holdPlayerAnim;
    public Animator holdBuddyAnim;
    public Transform holdPairFeet;

    [Header("Settings")]
    public float latchDistance = 1.5f;
    public Key holdKey = Key.L;
    public Vector3 holdPairOffsetRight = Vector3.zero;
    public Vector3 holdPairOffsetLeft = Vector3.zero;

    private bool isHolding = false;
    private bool isReachingOnly = false;

    public bool allowHolding = true;
    public bool IsHolding => isHolding;

    void Update()
    {
        if (buddyFollow != null)
            buddyFollow.TickTrust(isHolding, Time.deltaTime);

        if (player == null || buddy == null)
            return;

        if (Keyboard.current == null)
            return;

        if (!allowHolding)
            return;

        bool pressed = Keyboard.current[holdKey].wasPressedThisFrame;
        bool released = Keyboard.current[holdKey].wasReleasedThisFrame;
        bool held = Keyboard.current[holdKey].isPressed;

        float dist = Mathf.Abs(player.position.x - buddy.position.x);
        bool canLatch = dist <= latchDistance;

        if (pressed)
        {
            if (!isHolding)
            {
                if (canLatch)
                {
                    StartHolding();
                }
                else
                {
                    StartReachingOnly();
                }
            }
        }

        // while holding, if buddy becomes close enough, switch from reach to hold
        if (held && !isHolding && isReachingOnly && canLatch)
        {
            StopReachingOnly();
            StartHolding();
        }

        if (released)
        {
            if (isHolding)
                StopHolding();

            if (isReachingOnly)
                StopReachingOnly();
        }
    }

    void LateUpdate()
    {
        if (!isHolding || holdPair == null || player == null) return;

        bool facingLeft = false;
        if (playerSR != null)
            facingLeft = playerSR.flipX;

        Vector3 scale = holdPair.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingLeft ? -1f : 1f);
        holdPair.transform.localScale = scale;

        // first align the hold pair feet to the player position
        if (holdPairFeet != null)
        {
            Vector3 feetOffset = holdPair.transform.position - holdPairFeet.position;
            holdPair.transform.position = player.position + feetOffset;
        }

        // then apply final visual tweak offset
        holdPair.transform.position += facingLeft ? holdPairOffsetLeft : holdPairOffsetRight;

        bool walking = false;
        if (playerAnim != null)
            walking = playerAnim.GetBool("isWalking");

        if (holdPlayerAnim != null)
            holdPlayerAnim.SetBool("isWalking", walking);

        if (holdBuddyAnim != null)
            holdBuddyAnim.SetBool("isWalking", walking);
    }

    void StartHolding()
    {
        Debug.Log("startholding() actually ran");
        
        if (isReachingOnly)
            StopReachingOnly();

        isHolding = true;

        if (buddyFollow != null)
            buddyFollow.enabled = false;

        if (playerMovements != null)
            playerMovements.SetHandHolding(true);

        if (playerSR != null) playerSR.enabled = false;
        if (buddySR != null) buddySR.enabled = false;

        if (playerAnim != null) playerAnim.enabled = false;
        if (buddyAnim != null) buddyAnim.enabled = false;

        if (holdPair != null)
            holdPair.SetActive(true);
    }

    void StopHolding()
    {
        isHolding = false;

        if (buddy != null && player != null)
        {
            bool facingLeft = false;
            if (playerSR != null)
                facingLeft = playerSR.flipX;

            Vector3 releaseOffset = facingLeft ? new Vector3(2f, 0f, 0f) : new Vector3(-1.1f, 0f, 0f);
            
            buddy.position = new Vector3(
            player.position.x + releaseOffset.x,
            buddy.position.y,
            buddy.position.z
        );

        Rigidbody2D buddyRb = buddy.GetComponent<Rigidbody2D>();
        if (buddyRb != null)
            buddyRb.linearVelocity = Vector2.zero;

        }

        if (buddyFollow != null)
            buddyFollow.enabled = true;

        if (playerMovements != null)
            playerMovements.SetHandHolding(false);

        if (playerSR != null) playerSR.enabled = true;
        if (buddySR != null) buddySR.enabled = true;

        if (playerAnim != null) playerAnim.enabled = true;
        if (buddyAnim != null) buddyAnim.enabled = true;

        if (holdPair != null)
            holdPair.SetActive(false);

        if (playerMovements != null)
            playerMovements.SetReachingHand(false);

        if (playerAnim != null)
            playerAnim.SetBool("isReachingHand", false);
    }

    void StartReachingOnly()
        {
            isReachingOnly = true;

            if (playerMovements != null)
                playerMovements.SetReachingHand(true);

            if (playerAnim != null)
                playerAnim.SetBool("isReachingHand", true);
        }

        void StopReachingOnly()
        {
            isReachingOnly = false;

            if (playerMovements != null)
                playerMovements.SetReachingHand(false);

            if (playerAnim != null)
                playerAnim.SetBool("isReachingHand", false);
        }

    public void ForceStopHolding()
    {
        if (isHolding)
            StopHolding();
    }
}