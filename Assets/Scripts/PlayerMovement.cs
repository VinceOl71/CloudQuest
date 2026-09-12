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

    private float horizontal;
    private bool jumpPressed;
    private bool grounded;

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
            return;

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

        // Hold the press until the next physics step so it is never dropped
        // on a frame where FixedUpdate does not run
        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        // Animations
        anim.SetBool("run", horizontal != 0);
        anim.SetBool("grounded", grounded);
    }

    private void FixedUpdate()
    {
        grounded = IsGrounded();

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
        if (jumpPressed)
        {
            if (grounded)
            {
                Jump();
            }

            jumpPressed = false;
        }
    }

    private void Jump()
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
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
