using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // ─── Config ───────────────────────────────────────────────────
    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Header("Dust Effects")]
    [SerializeField] private ParticleSystem runDust;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem landDust;

    [Header("Collision Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private float wallOffsetX = 0.12f;
    [SerializeField] private float wallCheckDistance = 0.05f;

    // ─── Animator hashes ──────────────────────────────────────────
    static readonly int AnimRunning = Animator.StringToHash("isRunning");
    static readonly int AnimJumping = Animator.StringToHash("isJumping");
    static readonly int AnimFalling = Animator.StringToHash("isFalling");
    static readonly int AnimWallSlide = Animator.StringToHash("isTouchingWall");
    static readonly int AnimJumpCount = Animator.StringToHash("jumpCount");
    static readonly int AnimIdle = Animator.StringToHash("PlayerIdle");
    static readonly int AnimJump = Animator.StringToHash("PlayerJump");
    static readonly int AnimDblJump = Animator.StringToHash("PlayerDubbleJump");
    static readonly int AnimFall = Animator.StringToHash("PlayerFall");

    // ─── Components ───────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator animator;
    private GameManager gameManager;

    // ─── Hit & Death ──────────────────────────────────────────────
    private bool isDead = false;
    private bool isHit = false;

    // ─── Runtime state ────────────────────────────────────────────
    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isOnSand;
    private bool isFallingFromGround;
    private bool isTouchingWall;
    private bool wasTouchingWall;
    private bool wasWallJumping;
    private bool canWallJump = true;
    private int jumpCount;
    private float wallJumpTimer;

    // ─── Init ─────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    // ─── Main loop ────────────────────────────────────────────────
    private void Update()
    {
        if (isDead) return; // Không xử lý gì khi đã chết

        if (wallJumpTimer > 0f)
            wallJumpTimer -= Time.deltaTime;

        ReadInput();
        CheckGround();
        CheckSand();
        UpdateWallCheckPosition();
        CheckWall();
        HandleJump();
        HandleMovement();
        HandleWallSlide();
        UpdateAnimation();
        HandleDustEffects();
    }

    // ─── Input ────────────────────────────────────────────────────
    private void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    // ─── Ground / Sand / Wall detection ──────────────────────────
    private void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(
        groundCheck.position,
        new Vector2(0.8f, 0.1f),
        0f,
        groundLayer | sandLayer
    );

        if (isGrounded)
        {
            wasWallJumping = false;
            isFallingFromGround = false;

            if (rb.linearVelocity.y <= 0.1f)
                jumpCount = 0;
            if (!wasGrounded)
                animator.Play(AnimIdle);
        }
        else
        {
            if (wasGrounded && rb.linearVelocity.y <= 0f)
            {
                isFallingFromGround = true;
                jumpCount = config.maxJumps;
                animator.Play(AnimFall);
            }
        }
    }

    private void CheckSand()
    {
        isOnSand = Physics2D.OverlapBox(
            groundCheck.position,
            new Vector2(0.8f, 0.1f),
            0f,
            sandLayer
        );
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
        wasTouchingWall = isTouchingWall;

        Vector2 dir = new Vector2(transform.localScale.x, 0f);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, wallLayer);

        if (!isTouchingWall)
        {
            wasWallJumping = false;
            if (wasTouchingWall && !isGrounded)
                jumpCount = 0;
        }
    }

    // ─── Movement ────────────────────────────────────────────────
    private void HandleMovement()
    {
        if (isTouchingWall && !isGrounded && moveInput != 0f
            && Mathf.Sign(moveInput) == transform.localScale.x)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (wallJumpTimer > 0f) return;

        float speed = (isOnSand && isGrounded) ? config.sandSpeed : config.moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput > 0f) transform.localScale = Vector3.one;
        else if (moveInput < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    // ─── Jump ─────────────────────────────────────────────────────
    private void HandleJump()
    {
        if (!JumpPressed()) return;

        if (isTouchingWall && !isGrounded && canWallJump && !wasWallJumping)
        {
            ExecuteWallJump();
            return;
        }

        if (jumpCount < config.maxJumps)
            ExecuteJump();
    }

    private void ExecuteWallJump()
    {
        canWallJump = false;
        wasWallJumping = true;
        wallJumpTimer = config.wallJumpCooldown;
        jumpCount = config.maxJumps - 1;

        float dir = -transform.localScale.x;
        transform.localScale = new Vector3(dir, 1f, 1f);
        rb.linearVelocity = new Vector2(dir * config.wallJumpHorizontalForce, config.wallJumpForce);

        animator.Play(AnimJump);
        StartCoroutine(ResetWallJumpCooldown());
    }

    private void ExecuteJump()
    {
        jumpCount++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.jumpForce);
        animator.Play(jumpCount >= 2 ? AnimDblJump : AnimJump);
    }

    private IEnumerator ResetWallJumpCooldown()
    {
        yield return new WaitForSeconds(config.wallJumpCooldown);
        canWallJump = true;
    }

    private static bool JumpPressed() =>
        Input.GetButtonDown("Jump") ||
        Input.GetKeyDown(KeyCode.UpArrow) ||
        Input.GetKeyDown(KeyCode.W);

    // ─── Wall slide ───────────────────────────────────────────────
    private void HandleWallSlide()
    {
        if (!isTouchingWall || isGrounded || wasWallJumping) return;

        if (rb.linearVelocity.y < config.wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.wallSlideSpeed);
    }

    // ─── Animation ───────────────────────────────────────────────
    private void UpdateAnimation()
    {
        float vy = rb.linearVelocity.y;

        animator.SetBool(AnimRunning, Mathf.Abs(moveInput) > 0.1f && isGrounded);
        animator.SetBool(AnimJumping, vy > 0.1f && !isGrounded);
        animator.SetBool(AnimFalling, (vy < -0.1f && !isGrounded) || isFallingFromGround);
        animator.SetBool(AnimWallSlide, isTouchingWall && !isGrounded);
        animator.SetInteger(AnimJumpCount, jumpCount);
    }

    // ─── Dust effects ─────────────────────────────────────────────
    private void HandleDustEffects()
    {
        bool shouldRunDust = isGrounded && Mathf.Abs(moveInput) > 0.1f;
        if (shouldRunDust)
        {
            if (!runDust.isPlaying) runDust.Play();
        }
        else
        {
            runDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (wasGrounded && !isGrounded && rb.linearVelocity.y > 0f)
            jumpDust.Play();

        if (!wasGrounded && isGrounded)
            landDust.Play();
    }

    // ─── Damage & Death ───────────────────────────────────────────
    public void TakeDamage(float forceX = 0f, float forceY = 0f)
    {
        if (isDead || isHit) return;
        StartCoroutine(DieSequence(forceX, forceY));
    }

    private IEnumerator DieSequence(float forceX, float forceY)
    {
        isDead = true;
        isHit = true;

        // Phát animation chết
        animator.SetBool(AnimRunning, false);
        animator.SetBool(AnimJumping, false);
        animator.SetBool(AnimFalling, false);
        animator.SetBool(AnimWallSlide, false);
        animator.Play("PlayerHIt");

        // Tắt collider để xuyên qua sàn rơi xuống luôn
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Gravity mạnh + đẩy xuống ngay lập tức
        rb.gravityScale = 5f;
        rb.linearVelocity = new Vector2(forceX, forceY);

        // Chờ player rơi ra khỏi bản đồ (khoảng 1.5s)
        yield return new WaitForSeconds(1.5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
    