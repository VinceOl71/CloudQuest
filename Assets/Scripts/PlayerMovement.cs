using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private BoxCollider2D boxCollider;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    public float speed = 5f;
    public float jumpForce = 10f;

    [Header("Jump feel")]
    [Tooltip("How long after walking off a ledge a jump still counts.")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Tooltip("How long before landing a jump press is remembered, so it fires on touchdown instead of being swallowed.")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Tooltip("Share of upward speed kept when jump is released early. 0 cuts the jump dead, 1 turns variable height off.")]
    [Range(0f, 1f)]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Tooltip("Gravity is multiplied by this while falling. A symmetrical arc reads as floaty, so the way down is made quicker than the way up.")]
    [SerializeField] private float fallGravityMultiplier = 1.6f;

    [Tooltip("Fastest the player may fall, in units per second. A safety net for tall drops rather than something the current level reaches.")]
    [SerializeField] private float maxFallSpeed = 20f;

    private float horizontal;
    private bool jumpHeld;
    private bool grounded;

    private float coyoteCounter;
    private float jumpBufferCounter;

    // Starts spent, so nothing is trimmed before the first jump happens
    private bool jumpCut = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        body.freezeRotation = true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            horizontal = 0f;
            jumpHeld = false;
            return;
        }

        horizontal = 0f;

        if (keyboard.aKey.isPressed)
            horizontal = -1f;

        if (keyboard.dKey.isPressed)
            horizontal = 1f;

        // Flip sprite
        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;
        }

        // Remember the press for a moment. Reading the press here rather than
        // the held state is what stops a held key turning into repeat jumps.
        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        jumpHeld = keyboard.spaceKey.isPressed;

        // Animations
        anim.SetBool("run", horizontal != 0);
        anim.SetBool("grounded", grounded);
    }

    private void FixedUpdate()
    {
        grounded = IsGrounded();

        // Walking off a ledge opens a grace period rather than ending the jump outright
        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;
        jumpBufferCounter -= Time.fixedDeltaTime;

        float move = horizontal;

        // Holding a direction into a wall in mid-air is what supplies the
        // normal force that friction needs, which pins the player to the wall.
        // Dropping the input that pushes into it lets them fall instead.
        if (!grounded && move != 0f && IsTouchingWall(move))
        {
            move = 0f;
        }

        // Move player
        body.linearVelocity = new Vector2(move * speed, body.linearVelocity.y);

        // Jump
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();

            // Spend both, so a single press cannot become a second jump
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Letting go on the way up trims the arc, turning a tap into a short hop
        if (!jumpHeld && !jumpCut && body.linearVelocity.y > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x,
                                              body.linearVelocity.y * jumpCutMultiplier);
            jumpCut = true;
        }

        // Unity applies the base gravity itself during the step, so only the
        // difference is added here, and only on the way down
        if (!grounded && body.linearVelocity.y < 0f)
        {
            float extra = Physics2D.gravity.y * body.gravityScale
                          * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            body.linearVelocity += Vector2.up * extra;
        }

        if (body.linearVelocity.y < -maxFallSpeed)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, -maxFallSpeed);
        }
    }

    private void Jump()
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
        jumpCut = false;
    }

    private bool IsGrounded()
    {
        Bounds bounds = boxCollider.bounds;

        // Slightly narrower than the collider so the cast cannot catch a wall
        // the player is pressed up against
        Vector2 size = new Vector2(bounds.size.x * 0.9f, bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            size,
            0f,
            Vector2.down,
            0.1f,
            groundLayer
        );

        return hit.collider != null;
    }

    private bool IsTouchingWall(float direction)
    {
        Bounds bounds = boxCollider.bounds;

        // A thin box down the player's side, kept clear of their feet so the
        // floor they are standing on never counts as a wall
        Vector2 size = new Vector2(0.05f, bounds.size.y * 0.9f);

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            size,
            0f,
            direction < 0f ? Vector2.left : Vector2.right,
            bounds.extents.x + 0.05f,
            groundLayer.value | wallLayer.value
        );

        return hit.collider != null;
    }
}
