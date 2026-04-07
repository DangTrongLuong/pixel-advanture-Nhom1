using UnityEngine;
using System.Collections;

public enum RockMovementType
{
    Horizontal,     // moves left/right
    Vertical,       // moves up/down
    CircleCW,       // moves in 4 directions clockwise (up→right→down→left)
    CircleCCW       // moves in 4 directions counter-clockwise (down→left→up→right)
}

public class RockHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private RockMovementType movementType = RockMovementType.Horizontal;
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 1.2f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    
    // Shared
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isWaiting = false;
    private float obstacleCheckDistance;
    private float crushCheckDistance;
    
    // Horizontal/Vertical
    private int direction = 1;
    private float hitCooldownTimer = 0f;
    
    // Blink logic
    private float blinkTimer = 0f;
    [SerializeField] private float blinkIntervalMin = 3f;
    [SerializeField] private float blinkIntervalMax = 6f;
    
    // Circle movement
    private readonly Vector2[] directionsCircleCW = {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left
    };
    
    private readonly Vector2[] directionsCircleCCW = {
        Vector2.down, Vector2.left, Vector2.up, Vector2.right
    };
    
    private readonly string[] hitTriggersCircleCW = {
        "TopHit", "RightHit", "BottomHit", "LeftHit"
    };
    
    private readonly string[] hitTriggersCircleCCW = {
        "BottomHit", "LeftHit", "TopHit", "RightHit"
    };
    
    [SerializeField] private int startDirection = 0;
    private int currentDirIndex;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
        if (col != null)
        {
            if (movementType == RockMovementType.Horizontal)
            {
                obstacleCheckDistance = col.size.x / 2f + 0.05f;
                crushCheckDistance = col.size.x / 2f + 0.3f;
            }
            else if (movementType == RockMovementType.Vertical)
            {
                obstacleCheckDistance = col.size.y / 2f + 0.05f;
                crushCheckDistance = col.size.y / 2f + 0.3f;
            }
            else // Circle types
            {
                obstacleCheckDistance = col.size.x / 2f + 0.05f;
                crushCheckDistance = col.size.x / 2f;
            }
        }
        
        if (movementType == RockMovementType.Vertical)
        {
            direction = startDirection;
        }
        else if (movementType == RockMovementType.CircleCW || movementType == RockMovementType.CircleCCW)
        {
            currentDirIndex = startDirection;
        }
        else // Horizontal
        {
            direction = 1;
        }
    }

    void Update()
    {
        if (isWaiting) return;
        
        // Cooldown timer (for non-vertical types)
        if (movementType != RockMovementType.Vertical && hitCooldownTimer > 0)
        {
            hitCooldownTimer -= Time.deltaTime;
            return;
        }
        
        // Auto-blink logic
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            animator.SetTrigger("Blink");
            blinkTimer = Random.Range(blinkIntervalMin, blinkIntervalMax);
        }
        
        // Update velocity based on movement type
        if (movementType == RockMovementType.Horizontal)
        {
            float targetVelocity = direction * moveSpeed;
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVelocity, Time.deltaTime * 5f), 0f);
        }
        else if (movementType == RockMovementType.Vertical)
        {
            float targetVelocity = direction * moveSpeed;
            rb.linearVelocity = new Vector2(0f, Mathf.Lerp(rb.linearVelocity.y, targetVelocity, Time.deltaTime * 5f));
        }
        else // Circle types
        {
            Vector2 targetVelocity = GetCurrentDirection() * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 5f);
        }
        
        DetectAndMovePlayerOnTop();
        CheckObstacle();
    }

    private Vector2 GetCurrentDirection()
    {
        if (movementType == RockMovementType.CircleCW)
            return directionsCircleCW[currentDirIndex];
        else if (movementType == RockMovementType.CircleCCW)
            return directionsCircleCCW[currentDirIndex];
        else
            return Vector2.zero;
    }

    private void DetectAndMovePlayerOnTop()
    {
        Vector2 checkPos = transform.position + (Vector3)Vector2.up * (col.size.y / 2f + 0.3f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            checkPos,
            new Vector2(col.size.x * 1.2f, 0.5f),
            0f,
            LayerMask.GetMask("Player")
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    if (playerRb.linearVelocity.y < -0.1f) return;
                    
                    if (movementType == RockMovementType.Horizontal)
                    {
                        playerRb.linearVelocity = new Vector2(rb.linearVelocity.x, playerRb.linearVelocity.y);
                    }
                    else if (movementType == RockMovementType.Vertical)
                    {
                        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, rb.linearVelocity.y);
                    }
                    else // Circle types
                    {
                        Vector2 moveDir = GetCurrentDirection();
                        playerRb.linearVelocity = new Vector2(
                            Mathf.Abs(moveDir.x) > 0.5f ? rb.linearVelocity.x : playerRb.linearVelocity.x,
                            Mathf.Abs(moveDir.y) > 0.5f ? rb.linearVelocity.y : playerRb.linearVelocity.y
                        );
                    }
                }
                break;
            }
        }
    }

    void CheckObstacle()
    {
        LayerMask obstacleLayer = wallLayer | groundLayer;
        Vector2 origin = new Vector2(
            transform.position.x + col.offset.x,
            transform.position.y + col.offset.y
        );

        Vector2 rayDirection;
        if (movementType == RockMovementType.Horizontal)
            rayDirection = new Vector2(direction, 0f);
        else if (movementType == RockMovementType.Vertical)
            rayDirection = new Vector2(0f, direction);
        else
            rayDirection = GetCurrentDirection();

        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null && !isWaiting)
            StartCoroutine(HitObstacle(rayDirection));
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (isWaiting) return;

        float rockHeadTopY = transform.position.y + col.size.y / 2f;
        bool playerOnTop = other.transform.position.y > rockHeadTopY;
        
        if (playerOnTop)
        {
            LayerMask obstacleLayer = wallLayer | groundLayer;
            Vector2 rayDirection;
            
            if (movementType == RockMovementType.Horizontal)
                rayDirection = Vector2.up;
            else if (movementType == RockMovementType.Vertical)
                rayDirection = direction > 0 ? Vector2.up : Vector2.down;
            else
                rayDirection = GetCurrentDirection();

            RaycastHit2D obstacleAhead = Physics2D.Raycast(
                transform.position,
                rayDirection,
                1.0f,
                obstacleLayer
            );
            
            if (obstacleAhead.collider != null)
            {
                other.gameObject.GetComponent<PlayerController>()
                    ?.TakeDamage(0f, 10f);
            }
            return;
        }

        LayerMask obstacleLayer2 = wallLayer | groundLayer;
        Vector2 crushRayDirection;
        
        if (movementType == RockMovementType.Horizontal)
            crushRayDirection = new Vector2(direction, 0f);
        else if (movementType == RockMovementType.Vertical)
            crushRayDirection = new Vector2(0f, direction);
        else
            crushRayDirection = GetCurrentDirection();

        RaycastHit2D obstacleBehind = Physics2D.Raycast(
            other.transform.position,
            crushRayDirection,
            crushCheckDistance,
            obstacleLayer2
        );

        if (obstacleBehind.collider != null)
        {
            if (movementType == RockMovementType.Horizontal)
            {
                float knockDir = -direction;
                other.gameObject.GetComponent<PlayerController>()
                    ?.TakeDamage(knockDir * 12f, 10f);
            }
            else if (movementType == RockMovementType.Vertical)
            {
                float knockY = -direction * 12f;
                other.gameObject.GetComponent<PlayerController>()
                    ?.TakeDamage(0f, knockY);
            }
            else // Circle types
            {
                Vector2 knockback = -GetCurrentDirection() * 12f;
                other.gameObject.GetComponent<PlayerController>()
                    ?.TakeDamage(knockback.x, knockback.y);
            }
        }
    }

    IEnumerator HitObstacle(Vector2 hitDirection)
    {
        Debug.Log("🚀 HitObstacle STARTED");
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.15f, 0.08f);
        }

        animator.speed = hitAnimSpeed;
        animator.SetTrigger("Blink");
        Debug.Log("▶️ SetTrigger(Blink) called");

        yield return new WaitForSeconds(0.2f);

        // Update direction/index
        if (movementType == RockMovementType.Horizontal)
        {
            direction *= -1;
            Debug.Log($"🔄 Direction changed to: {direction}");
        }
        else if (movementType == RockMovementType.Vertical)
        {
            direction *= -1;
            Debug.Log($"🔄 Direction changed to: {direction}");
        }
        else // Circle types
        {
            currentDirIndex = (currentDirIndex + 1) % 4;
            Debug.Log($"🔄 Direction index changed to: {currentDirIndex}");
        }

        // Play hit animation based on the direction we were moving when we hit
        string hitStateName = GetHitAnimFromDirection(hitDirection);
        animator.Play(hitStateName, 0, 0f);
        Debug.Log($"▶️ animator.Play('{hitStateName}') called");
        Debug.Log($"✅ IsName('{hitStateName}'): {animator.GetCurrentAnimatorStateInfo(0).IsName(hitStateName)}");

        yield return new WaitForSeconds(0.3f);

        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (movementType != RockMovementType.Vertical)
            hitCooldownTimer = 0.8f;
        
        // Reset blink timer after hit
        blinkTimer = Random.Range(blinkIntervalMin, blinkIntervalMax);
        
        isWaiting = false;
        Debug.Log("✨ HitObstacle COMPLETE");
    }

    private string GetHitAnimFromDirection(Vector2 hitDirection)
    {
        // Normalize the direction for reliable comparison
        hitDirection = hitDirection.normalized;
        
        // Check vertical direction first (more reliable for Up/Down)
        if (hitDirection.y > 0.5f)
            return "TopHit";
        else if (hitDirection.y < -0.5f)
            return "BottomHit";
        // Then check horizontal direction
        else if (hitDirection.x > 0.5f)
            return "RightHit";
        else if (hitDirection.x < -0.5f)
            return "LeftHit";
        
        // Fallback (shouldn't happen)
        return "RightHit";
    }

    float GetClipLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        return 0.3f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if (movementType == RockMovementType.Horizontal)
        {
            Gizmos.DrawLine(transform.position,
                transform.position + new Vector3(direction * obstacleCheckDistance, 0f, 0f));
        }
        else if (movementType == RockMovementType.Vertical)
        {
            Gizmos.DrawLine(transform.position,
                transform.position + new Vector3(0f, direction * obstacleCheckDistance, 0f));
        }
        else // Circle types
        {
            Vector2 dir = GetCurrentDirection();
            Gizmos.DrawLine(transform.position,
                (Vector2)transform.position + dir * obstacleCheckDistance);
        }
        
        Gizmos.color = Color.yellow;
        
        if (movementType == RockMovementType.Horizontal)
        {
            Gizmos.DrawLine(transform.position,
                transform.position + new Vector3(direction * crushCheckDistance, 0f, 0f));
        }
        else if (movementType == RockMovementType.Vertical)
        {
            Gizmos.DrawLine(transform.position,
                transform.position + new Vector3(0f, direction * crushCheckDistance, 0f));
        }
        else // Circle types
        {
            Vector2 dir = GetCurrentDirection();
            Gizmos.DrawLine(transform.position,
                (Vector2)transform.position + dir * crushCheckDistance);
        }
    }
}