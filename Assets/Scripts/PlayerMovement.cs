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
        float horizontal = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        // Flip sprite
        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;
        }

        // Move player
        body.linearVelocity = new Vector2(horizontal * speed, body.linearVelocity.y);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            Jump();
        }

        // Animations
        anim.SetBool("run", horizontal != 0);
        anim.SetBool("grounded", IsGrounded());
    }

    private void Jump()
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
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