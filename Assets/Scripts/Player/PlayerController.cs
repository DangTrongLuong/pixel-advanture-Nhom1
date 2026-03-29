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

<<<<<<< HEAD
    [Header("Hit & Death")]
    private bool isDead = false;
    private bool isHit = false;

=======
    // ─── Components ───────────────────────────────────────────────
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
    private Rigidbody2D rb;
    private Animator animator;
    private GameManager gameManager;

<<<<<<< HEAD
    private float normalSpeed;

    private bool isGrounded;
    private bool isTouchingWall;

    private bool wasWallJumping = false;
    private bool canWallJump = true;
    private bool isOnSand;

    private float moveInput;
    private float wallJumpTimer = 0f;

    private int jumpCount = 0;
    [SerializeField] private int maxJumps = 1;
=======
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
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8

    // ─── Init ─────────────────────────────────────────────────────
    private void Awake()
<<<<<<< HEAD
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
        normalSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isDead) return; // không xử lý gì khi đã chết

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
=======
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    // ─── Main loop ────────────────────────────────────────────────
    private void Update()
    {
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
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
    private void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

<<<<<<< HEAD
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
=======
    // ─── Ground / Sand / Wall detection ──────────────────────────
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
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
<<<<<<< HEAD

            if (wasWallBefore && !isGrounded)
=======
            if (wasTouchingWall && !isGrounded)
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
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
<<<<<<< HEAD
        if (isTouchingWall && !isGrounded && !wasWallJumping)
        {
            if (rb.linearVelocity.y < wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
        }
=======
        if (!isTouchingWall || isGrounded || wasWallJumping) return;

        if (rb.linearVelocity.y < config.wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.wallSlideSpeed);
>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
    }

    // ─── Animation ───────────────────────────────────────────────
    private void UpdateAnimation()
    {
        float vy = rb.linearVelocity.y;

<<<<<<< HEAD
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
            moveSpeed = sandSpeed;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sand"))
            moveSpeed = normalSpeed;
    }

    // -------------------------------------------------------------
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
    animator.SetBool("isRunning", false);
    animator.SetBool("isJumping", false);
    animator.SetBool("isFalling", false);
    animator.SetBool("isTouchingWall", false);
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
=======
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


>>>>>>> 27faaab000addfea2669a5b1fd1d3d7f7ec6e2c8
}