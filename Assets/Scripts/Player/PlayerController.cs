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

    [Header("Collision Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallOffsetX = 0.12f;
    [SerializeField] private float wallCheckDistance = 0.05f;

    private LayerMask trapLayer;

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
    private GameManager gameManager;

    private bool isDead = false;
    private bool isHit = false;

    private float moveInput;
    private float joystickInput;
    private float keyboardInput;

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

    public System.Action<float> OnMoveInputChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
        trapLayer = LayerMask.GetMask("Traps");
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
        UpdateWallCheckPosition();
        CheckWall();
        HandleJump();
        HandleMovement();
        HandleWallSlide();
        UpdateAnimation();
        HandleDustEffects();
    }

    public bool IsGrounded => isGrounded;
    public float MoveInput => moveInput;

    public void SetMoveInput(float value) => joystickInput = value;

    public void MobileJump()
    {
        if (isTouchingWall && !isGrounded && canWallJump && !wasWallJumping)
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
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position, new Vector2(0.8f, 0.1f), 0f,
            groundLayer | sandLayer | trapLayer);

        if (isGrounded)
        {
            wasWallJumping = false;
            isFallingFromGround = false;
            if (rb.linearVelocity.y <= 0.1f) jumpCount = 0;
            if (!wasGrounded) animator.Play(AnimIdle);
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
            groundCheck.position, new Vector2(0.8f, 0.1f), 0f, sandLayer);
    }

    private void UpdateWallCheckPosition()
    {
        wallCheck.position = new Vector3(
            transform.position.x + wallOffsetX * transform.localScale.x,
            transform.position.y,
            transform.position.z);
    }

    private void CheckWall()
    {
        wasTouchingWall = isTouchingWall;
        isTouchingWall = false;

        Vector2 dir = new Vector2(transform.localScale.x, 0f);
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, wallLayer | trapLayer);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Traps"))
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
    }

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

    private void HandleJump()
    {
        if (!JumpPressed()) return;

        if (isTouchingWall && !isGrounded && canWallJump && !wasWallJumping)
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
        if (!isTouchingWall || isGrounded || wasWallJumping) return;
        if (rb.linearVelocity.y < config.wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.wallSlideSpeed);
    }

    private void UpdateAnimation()
    {
        float vy = rb.linearVelocity.y;
        animator.SetBool(AnimRunning, Mathf.Abs(moveInput) > 0.1f && isGrounded);
        animator.SetBool(AnimJumping, vy > 0.1f && !isGrounded);
        animator.SetBool(AnimFalling, !isGrounded && (vy < -0.1f || isFallingFromGround));
        animator.SetBool(AnimWallSlide, isTouchingWall && !isGrounded);
        animator.SetInteger(AnimJumpCount, jumpCount);
    }

    private void HandleDustEffects()
    {
        bool shouldRunDust = isGrounded && Mathf.Abs(moveInput) > 0.1f;
        if (shouldRunDust) { if (!runDust.isPlaying) runDust.Play(); }
        else runDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (wasGrounded && !isGrounded && rb.linearVelocity.y > 0f) jumpDust.Play();
        if (!wasGrounded && isGrounded) landDust.Play();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<RockHead>() != null ||
            collision.gameObject.GetComponent<RockHeadVertical>() != null ||
            collision.gameObject.GetComponent<RockHeadCircle>() != null ||
            collision.gameObject.GetComponent<RockHeadCircleReverse>() != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true; isFallingFromGround = false; wasWallJumping = false;
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

        var deathHandler = GetComponent<PlayerDeathHandler>();
        if (deathHandler != null) deathHandler.Die(forceX, forceY);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector2(0.8f, 0.1f));
    }
}