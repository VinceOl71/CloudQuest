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

        // Move player
        body.linearVelocity = new Vector2(horizontal * speed, body.linearVelocity.y);

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

    private bool IsTouchingWall()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            spriteRenderer.flipX ? Vector2.left : Vector2.right,
            0.1f,
            wallLayer
        );

        return hit.collider != null;
    }
}
