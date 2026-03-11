using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private float sandSpeed = 2f;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallOffsetX = 0.12f;
    [SerializeField] private float wallCheckDistance = 0.05f;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = -1.5f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallJumpCooldown = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private GameManager gameManager;

    private float normalSpeed; 

    private bool isGrounded;
    private bool isTouchingWall;

    private bool wasWallJumping = false;
    private bool canWallJump = true;
    private bool isOnSand;

    private float moveInput;
    private float wallJumpTimer = 0f;

    // DOUBLE JUMP FIXED
    private int jumpCount = 0;
    [SerializeField] private int maxJumps = 1; 

    private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    gameManager = FindAnyObjectByType<GameManager>();

    normalSpeed = moveSpeed; // thêm dòng này
}


    private void Update()
    {
        ReadInput();
        HandleMovement();
        CheckGround();
        UpdateWallCheckPosition();
        CheckWall();
        HandleWallSlide();
        HandleJump();
        UpdateAnimation();
        CheckSand();

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.deltaTime;
    }


    // -------------------------------------------------------------
    private void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    private void HandleMovement()
    {
        if (isTouchingWall && !isGrounded && moveInput != 0 && Mathf.Sign(moveInput) == transform.localScale.x)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (wallJumpTimer > 0)
            return;

        float speed = isOnSand ? sandSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = Vector3.one;
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // -------------------------------------------------------------
    private void HandleJump()
    {
        if (!(Input.GetButtonDown("Jump")
          || Input.GetKeyDown(KeyCode.UpArrow)
          || Input.GetKeyDown(KeyCode.W)))
            return;

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
            jumpCount = 0;

        if (isTouchingWall && canWallJump && !wasWallJumping)
        {
            canWallJump = false;
            wasWallJumping = true;
            wallJumpTimer = wallJumpCooldown;

            float dir = -transform.localScale.x;
            transform.localScale = new Vector3(dir, 1, 1);

            rb.linearVelocity = new Vector2(
                dir * wallJumpHorizontalForce,
                wallJumpForce
            );

            jumpCount = 1;

            animator.Play("PlayerJump");

            StartCoroutine(WallJumpCooldown());
            return;
        }

        // DOUBLE JUMP FIX
        if (jumpCount < maxJumps)
        {
            jumpCount++;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (jumpCount == 1)
                animator.Play("PlayerJump");
            else if (jumpCount == 2)
                animator.Play("PlayerDubbleJump");
        }
    }

    private IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(wallJumpCooldown);
        canWallJump = true;
    }

    // -------------------------------------------------------------
    private void CheckGround()
    {
        bool wasGroundedState = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.12f,
            groundLayer | sandLayer
        );

        if (isGrounded)
        {
            wasWallJumping = false;

            // RESET jumpCount đúng lúc
            if (rb.linearVelocity.y <= 0.1f)
                jumpCount = 0;

            animator.SetInteger("jumpCount", 0);

            if (!wasGroundedState)
                animator.Play("PlayerIdle");
        }
    }

    private void UpdateWallCheckPosition()
    {
        wallCheck.position = new Vector3(
            transform.position.x + wallOffsetX * transform.localScale.x,
            transform.position.y,
            transform.position.z
        );
    }

    private void CheckWall()
    {
        bool wasWallBefore = isTouchingWall;

        Vector2 dir = new Vector2(transform.localScale.x, 0);
        isTouchingWall = Physics2D.Raycast(
            wallCheck.position,
            dir,
            wallCheckDistance,
            wallLayer
        );

        if (!isTouchingWall)
        {
            wasWallJumping = false;

            // FIX: Reset jumpCount khi bật ra khỏi tường
            if (wasWallBefore && !isGrounded)
            {
                jumpCount = 0;
            }
        }
    }

    // -------------------------------------------------------------
    private void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && !wasWallJumping)
        {
            if (rb.linearVelocity.y < wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
            }
        }
    }

    // -------------------------------------------------------------
    private void UpdateAnimation()
    {
        float vy = rb.linearVelocity.y;

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f && isGrounded);

        animator.SetBool("isJumping", vy > 0.1f && !isGrounded);

        animator.SetBool("isFalling", vy < -0.1f && !isGrounded);

        animator.SetBool("isTouchingWall", isTouchingWall && !isGrounded);
        animator.SetInteger("jumpCount", jumpCount);
    }

    void CheckSand()
{
    isOnSand = Physics2D.OverlapCircle(
        groundCheck.position,
        0.4f,
        sandLayer
    );
}

void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.layer == LayerMask.NameToLayer("Sand"))
    {
        moveSpeed = sandSpeed;
    }
}

void OnCollisionExit2D(Collision2D collision)
{
    if (collision.gameObject.layer == LayerMask.NameToLayer("Sand"))
    {
        moveSpeed = normalSpeed;
    }
}

}