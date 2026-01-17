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
    private AudioManager audioManager;

    private bool isGrounded;
    private bool isTouchingWall;

    private bool wasWallJumping = false;
    private bool canWallJump = true;

    private float moveInput;
    private float wallJumpTimer = 0f;

    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        // Nếu đang dính tường và đè vào tường → không được di chuyển vào
        if (isTouchingWall && !isGrounded && moveInput != 0 && Mathf.Sign(moveInput) == transform.localScale.x)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Trong thời gian wall jump cooldown → không override tốc độ
        if (wallJumpTimer > 0)
            return;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (moveInput > 0) transform.localScale = Vector3.one;
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // -------------------------------------------------------------
    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump"))
            
            return;

        // Jump bình thường
        if (isGrounded)
        {
            //audioManager.PlayJumpSound();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            wasWallJumping = false;
            return;
        }

        // WALL JUMP
        if (isTouchingWall && canWallJump && !wasWallJumping)
        {
            //audioManager.PlayJumpSound();
            canWallJump = false;
            wasWallJumping = true;
            wallJumpTimer = wallJumpCooldown;

            float dir = -transform.localScale.x;

            // Flip hướng
            transform.localScale = new Vector3(dir, 1, 1);

            rb.linearVelocity = new Vector2(
                dir * wallJumpHorizontalForce,
                wallJumpForce
            );

            StartCoroutine(WallJumpCooldown());
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
            groundLayer
        );

        if (isGrounded && !wasGroundedState)
        {
            wasWallJumping = false;
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
        Vector2 dir = new Vector2(transform.localScale.x, 0);
        bool previous = isTouchingWall;

        isTouchingWall = Physics2D.Raycast(
            wallCheck.position,
            dir,
            wallCheckDistance,
            wallLayer
        );

        if (previous && !isTouchingWall)
        {
            wasWallJumping = false;
        }
    }

    // -------------------------------------------------------------
    private void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && !wasWallJumping)
        {
            // Tốc độ trượt luôn cố định
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

        // Running
        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f && isGrounded);

        // Jumping
        animator.SetBool("isJumping", vy > 0.1f && !isGrounded);

        // Falling
        animator.SetBool("isFalling", vy < -0.1f && !isGrounded && !isTouchingWall);

        // Wall Sliding
        animator.SetBool("isTouchingWall", isTouchingWall && !isGrounded);
    }
}
