using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Header("Dust Effects")]
    [SerializeField] private ParticleSystem runDust;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem landDust;

    [Header("Sand Dust Effects")]
    [SerializeField] private ParticleSystem runDustSand;
    [SerializeField] private ParticleSystem jumpDustSand;
    [SerializeField] private ParticleSystem landDustSand;

    [Header("Collision Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private LayerMask iceLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallOffsetX = 0.12f;
    [SerializeField] private float wallCheckDistance = 0.02f;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Ice Slide")]
    [SerializeField] private Sprite iceSlideSprite;
    [SerializeField] private float iceIdleTimeBeforeSlide = 0.5f;

    private LayerMask trapLayer;
    private LayerMask allGroundMask;

    static readonly int AnimRunning = Animator.StringToHash("isRunning");
    static readonly int AnimJumping = Animator.StringToHash("isJumping");
    static readonly int AnimFalling = Animator.StringToHash("isFalling");
    static readonly int AnimWallSlide = Animator.StringToHash("isTouchingWall");
    static readonly int AnimJumpCount = Animator.StringToHash("jumpCount");
    static readonly int AnimIdle = Animator.StringToHash("PlayerIdle");
    static readonly int AnimJump = Animator.StringToHash("PlayerJump");
    static readonly int AnimDblJump = Animator.StringToHash("PlayerDubbleJump");
    static readonly int AnimFall = Animator.StringToHash("PlayerFall");

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    private bool isDead = false;
    private bool isHit = false;

    private float moveInput;
    private float joystickInput;
    private float keyboardInput;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isOnSand;
    private bool wasOnSand;
    private bool isOnIce;
    private float iceGraceTimer;
    private const float IceGrace = 0.1f;
    private bool isOnSwamp;
    private bool isFallingFromGround;

    private float groundedGraceTimer;
    private const float GroundedGrace = 0.12f;

    private bool isTouchingWall;
    private bool wasTouchingWall;
    private bool wasWallJumping;
    private bool canWallJump = true;
    private int jumpCount;
    private float wallJumpTimer;

    // ── ICE SLIDE STATE ──────────────────────────────────────────────────────
    private enum IceState { None, WaitingToSlide, Sliding }
    private IceState iceState = IceState.None;
    private float iceIdleTimer = 0f;
    private float iceSlideDirection = 1f;
    private Sprite originalSprite;
    // ─────────────────────────────────────────────────────────────────────────

    public System.Action<float> OnMoveInputChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindAnyObjectByType<GameManager>();
        trapLayer = LayerMask.GetMask("Traps");
        allGroundMask = groundLayer | sandLayer | iceLayer | trapLayer;

        if (spriteRenderer != null)
            originalSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        if (isDead) return;

        if (wallJumpTimer > 0f)
            wallJumpTimer -= Time.deltaTime;

        ReadKeyboard();
        ResolveMoveInput();
        CheckGround();
        CheckSand();
        CheckIce();
        UpdateWallCheckPosition();
        CheckWall();
        HandleIceSlide();
        HandleJump();
        HandleMovement();
        HandleWallSlide();
        UpdateAnimation();
        HandleDustEffects();
    }

    public void SetTerrain(TerrainZone.TerrainType type, bool isActive)
    {
        if (type == TerrainZone.TerrainType.Swamp) isOnSwamp = isActive;
    }

    public bool IsGrounded => isGrounded;
    public float MoveInput => moveInput;

    public void SetMoveInput(float value) => joystickInput = value;

    public void MobileJump()
    {
        bool trulyAirborne = !isGrounded && groundedGraceTimer <= 0f;
        if (isTouchingWall && trulyAirborne && canWallJump && !wasWallJumping)
        {
            ExecuteWallJump();
            return;
        }
        if (jumpCount < config.maxJumps)
            ExecuteJump();
    }

    private void ReadKeyboard()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (h == 0f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
        }
        keyboardInput = h;
    }

    private void ResolveMoveInput()
    {
        float prev = moveInput;
        moveInput = (keyboardInput != 0f) ? keyboardInput : joystickInput;
        if (!Mathf.Approximately(moveInput, prev))
            OnMoveInputChanged?.Invoke(moveInput);
    }

    private void CheckGround()
    {
        wasGrounded = isGrounded;

        bool physicsGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, allGroundMask);

        if (physicsGrounded)
        {
            groundedGraceTimer = GroundedGrace;
            isGrounded = true;
        }
        else
        {
            groundedGraceTimer -= Time.deltaTime;
            isGrounded = groundedGraceTimer > 0f;
        }

        if (isGrounded)
        {
            wasWallJumping = false;
            isFallingFromGround = false;
            if (rb.linearVelocity.y <= 0.1f) jumpCount = 0;
            if (!wasGrounded)
            {
                animator.Play(AnimIdle);
                if (isOnSand) landDustSand.Play();
                else landDust.Play();
            }
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
        wasOnSand = isOnSand;
        isOnSand = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, sandLayer);
    }

    private void CheckIce()
    {
        bool physicsOnIce = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, iceLayer);

        if (physicsOnIce)
        {
            iceGraceTimer = IceGrace;
            isOnIce = true;
        }
        else
        {
            iceGraceTimer -= Time.deltaTime;
            isOnIce = iceGraceTimer > 0f;
        }

        if (!isGrounded && groundedGraceTimer <= 0f)
        {
            iceGraceTimer = 0f;
            isOnIce = false;
        }
    }

    // ── ICE SLIDE LOGIC ──────────────────────────────────────────────────────
    private void HandleIceSlide()
    {
        if (!isOnIce || !isGrounded)
        {
            ExitIceSlide();
            return;
        }

        bool playerMoving = Mathf.Abs(moveInput) > 0.1f;

        switch (iceState)
        {
            case IceState.None:
                if (!playerMoving)
                {
                    iceState = IceState.WaitingToSlide;
                    iceIdleTimer = 0f;
                }
                break;

            case IceState.WaitingToSlide:
                if (playerMoving)
                {
                    iceState = IceState.None;
                }
                else
                {
                    iceIdleTimer += Time.deltaTime;
                    if (iceIdleTimer >= iceIdleTimeBeforeSlide)
                    {
                        iceState = IceState.Sliding;
                        iceSlideDirection = transform.localScale.x;
                        SetIceSlideSprite(true);
                    }
                }
                break;

            case IceState.Sliding:
                if (playerMoving)
                {
                    ExitIceSlide();
                }
                break;
        }
    }

    private void ExitIceSlide()
    {
        if (iceState == IceState.None) return;
        iceState = IceState.None;
        iceIdleTimer = 0f;
        SetIceSlideSprite(false);
    }

    private void SetIceSlideSprite(bool sliding)
    {
        if (spriteRenderer == null) return;
        if (sliding && iceSlideSprite != null)
            spriteRenderer.sprite = iceSlideSprite;
        else
            spriteRenderer.sprite = originalSprite;
    }

    public bool IsIceSliding => iceState == IceState.Sliding;
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateWallCheckPosition()
    {
        wallCheck.position = new Vector3(
            transform.position.x + wallOffsetX * transform.localScale.x,
            transform.position.y + 0.3f,
            transform.position.z);
    }

    private void CheckWall()
    {
        wasTouchingWall = isTouchingWall;
        isTouchingWall = false;

        if (isGrounded || groundedGraceTimer > 0f) return;

        Vector2 dir = new Vector2(transform.localScale.x, 0f);
        LayerMask wallMask = (wallLayer | trapLayer) & ~sandLayer & ~groundLayer & ~iceLayer;
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position, dir, wallCheckDistance, wallMask);

        if (hit.collider != null)
        {
            int hitLayer = hit.collider.gameObject.layer;
            if (((1 << hitLayer) & (sandLayer | groundLayer | iceLayer)) != 0)
                return;

            if (hitLayer == LayerMask.NameToLayer("Traps"))
            {
                float rotZ = hit.collider.transform.eulerAngles.z;
                if (rotZ > 180f) rotZ -= 360f;

                isTouchingWall = Mathf.Abs(rotZ - (-90f)) < 45f ||
                                 Mathf.Abs(rotZ - (-270f)) < 45f ||
                                 Mathf.Abs(rotZ - 90f) < 45f;
            }
            else
            {
                isTouchingWall = true;
            }
        }

        if (!isTouchingWall)
        {
            wasWallJumping = false;
            if (wasTouchingWall && !isGrounded) jumpCount = 0;
        }

        Debug.DrawRay(wallCheck.position, dir * wallCheckDistance, Color.red);
    }

    private void HandleMovement()
    {
        bool trulyAirborne = !isGrounded && groundedGraceTimer <= 0f;

        if (isTouchingWall && trulyAirborne && moveInput != 0f
            && Mathf.Sign(moveInput) == transform.localScale.x)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (wallJumpTimer > 0f && trulyAirborne) return;

        if (iceState == IceState.Sliding)
        {
            rb.linearVelocity = new Vector2(iceSlideDirection * config.moveSpeed, rb.linearVelocity.y);
            return;
        }

        float targetVelocityX;

        if (isGrounded)
        {
            if (isOnSwamp)
                targetVelocityX = 0f;
            else if (isOnSand)
                targetVelocityX = moveInput * config.sandSpeed;
            else
                targetVelocityX = moveInput * config.moveSpeed;
        }
        else
        {
            targetVelocityX = moveInput * config.moveSpeed;
        }

        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

        if (!isOnSwamp)
        {
            if (moveInput > 0f) transform.localScale = Vector3.one;
            else if (moveInput < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void HandleJump()
    {
        if (!JumpPressed()) return;

        bool trulyAirborne = !isGrounded && groundedGraceTimer <= 0f;

        if (iceState == IceState.Sliding) ExitIceSlide();

        if (isTouchingWall && trulyAirborne && canWallJump && !wasWallJumping)
        {
            ExecuteWallJump();
            return;
        }
        if (jumpCount < config.maxJumps) ExecuteJump();
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

        if (isOnSand) jumpDustSand.Play();
        else jumpDust.Play();
    }

    private IEnumerator ResetWallJumpCooldown()
    {
        yield return new WaitForSeconds(config.wallJumpCooldown);
        canWallJump = true;
    }

    private static bool JumpPressed()
    {
        return Input.GetButtonDown("Jump")
            || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.W);
    }

    private void HandleWallSlide()
    {
        bool trulyAirborne = !isGrounded && groundedGraceTimer <= 0f;
        if (!isTouchingWall || !trulyAirborne || wasWallJumping) return;
        if (rb.linearVelocity.y < config.wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.wallSlideSpeed);
    }

    private void UpdateAnimation()
    {
        float vy = rb.linearVelocity.y;
        bool sliding = iceState == IceState.Sliding;

        animator.SetBool(AnimRunning, !sliding && Mathf.Abs(moveInput) > 0.1f && isGrounded);
        animator.SetBool(AnimJumping, vy > 0.1f && !isGrounded);
        animator.SetBool(AnimFalling, !isGrounded && (vy < -0.1f || isFallingFromGround));
        animator.SetBool(AnimWallSlide, isTouchingWall && !isGrounded);
        animator.SetInteger(AnimJumpCount, jumpCount);
    }

    private void HandleDustEffects()
    {
        bool shouldRunDust = isGrounded && Mathf.Abs(moveInput) > 0.1f && !IsIceSliding;
        if (isOnSand)
        {
            runDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (shouldRunDust) { if (!runDustSand.isPlaying) runDustSand.Play(); }
            else runDustSand.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        else
        {
            runDustSand.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (shouldRunDust) { if (!runDust.isPlaying) runDust.Play(); }
            else runDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<RockHead>() != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    isFallingFromGround = false;
                    wasWallJumping = false;
                    groundedGraceTimer = GroundedGrace;
                    break;
                }
            }
        }
    }

    public void TakeDamage(float forceX = 0f, float forceY = 0f)
    {
        if (isDead || isHit) return;
        isDead = true;
        isHit = true;

        ExitIceSlide();
        var deathHandler = GetComponent<PlayerDeathHandler>();
        if (deathHandler != null) deathHandler.Die(forceX, forceY);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}