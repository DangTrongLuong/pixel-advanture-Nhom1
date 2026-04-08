using UnityEngine;
using System.Collections;

public enum SpikeMovementType
{
    Horizontal,
    Vertical,
    CircleCW
}

public class SpikeHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private SpikeMovementType movementType = SpikeMovementType.Horizontal;
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 0.6f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isWaiting = false;
    private float obstacleCheckDistance;
    private float crushCheckDistance;

    private int direction = 1;
    private float hitCooldownTimer = 0f;

    private readonly Vector2[] directionsCircleCW = {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left
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
            if (movementType == SpikeMovementType.Horizontal)
            {
                obstacleCheckDistance = col.size.x / 2f + 0.05f;
                crushCheckDistance = col.size.x / 2f + 0.3f;
            }
            else if (movementType == SpikeMovementType.Vertical)
            {
                obstacleCheckDistance = col.size.y / 2f + 0.05f;
                crushCheckDistance = col.size.y / 2f + 0.3f;
            }
            else
            {
                obstacleCheckDistance = col.size.x / 2f + 0.05f;
                crushCheckDistance = col.size.x / 2f + 0.3f;
            }
        }

        if (movementType == SpikeMovementType.Vertical)
            direction = startDirection;
        else if (movementType == SpikeMovementType.CircleCW)
            currentDirIndex = startDirection;
        else
            direction = 1;
    }

    void Update()
    {
        if (isWaiting) return;

        if (hitCooldownTimer > 0)
        {
            hitCooldownTimer -= Time.deltaTime;
            return;
        }

        if (movementType == SpikeMovementType.Horizontal)
            rb.linearVelocity = new Vector2(direction * moveSpeed, 0f);
        else if (movementType == SpikeMovementType.Vertical)
            rb.linearVelocity = new Vector2(0f, direction * moveSpeed);
        else
            rb.linearVelocity = directionsCircleCW[currentDirIndex] * moveSpeed;

        CheckObstacle();
    }

    private Vector2 GetCurrentDirection()
    {
        if (movementType == SpikeMovementType.CircleCW)
            return directionsCircleCW[currentDirIndex];
        return Vector2.zero;
    }

    void CheckObstacle()
    {
        LayerMask obstacleLayer = wallLayer | groundLayer;
        Vector2 origin = new Vector2(
            transform.position.x + col.offset.x,
            transform.position.y + col.offset.y
        );

        Vector2 rayDirection;
        if (movementType == SpikeMovementType.Horizontal)
            rayDirection = new Vector2(direction, 0f);
        else if (movementType == SpikeMovementType.Vertical)
            rayDirection = new Vector2(0f, direction);
        else
            rayDirection = GetCurrentDirection();

        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null && !isWaiting)
            StartCoroutine(Bounce(rayDirection));
    }

    // ✅ Detect tường nào bị chạm → chọn đúng animation
    private string GetHitAnimFromDirection(Vector2 moveDir)
    {
        float ax = Mathf.Abs(moveDir.x);
        float ay = Mathf.Abs(moveDir.y);

        if (ay > ax)
            return moveDir.y > 0 ? "TopHit" : "BottomHit";
        else
            return moveDir.x > 0 ? "RightHit" : "LeftHit";
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (isWaiting) return;

        LayerMask obstacleLayer = wallLayer | groundLayer;
        Vector2 crushRayDirection;

        if (movementType == SpikeMovementType.Horizontal)
            crushRayDirection = new Vector2(direction, 0f);
        else if (movementType == SpikeMovementType.Vertical)
            crushRayDirection = new Vector2(0f, direction);
        else
            crushRayDirection = GetCurrentDirection();

        RaycastHit2D obstacleBehind = Physics2D.Raycast(
            other.transform.position,
            crushRayDirection,
            crushCheckDistance,
            obstacleLayer
        );

        if (obstacleBehind.collider != null)
        {
            if (movementType == SpikeMovementType.Horizontal)
                other.gameObject.GetComponent<PlayerController>()?.TakeDamage(-direction * 12f, 10f);
            else if (movementType == SpikeMovementType.Vertical)
                other.gameObject.GetComponent<PlayerController>()?.TakeDamage(0f, -direction * 12f);
            else
            {
                Vector2 knockback = -GetCurrentDirection() * 12f;
                other.gameObject.GetComponent<PlayerController>()?.TakeDamage(knockback.x, knockback.y);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        Vector2 knockDir = (other.transform.position - transform.position).normalized;
        other.gameObject.GetComponent<PlayerController>()
            ?.TakeDamage(knockDir.x * 12f, knockDir.y * 8f);
    }

    IEnumerator Bounce(Vector2 hitDirection)
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.15f, 0.08f);

        animator.speed = hitAnimSpeed;
        animator.SetTrigger("Blink");

        yield return new WaitForSeconds(0.2f);

        if (movementType == SpikeMovementType.Horizontal)
        {
            direction *= -1;
        }
        else if (movementType == SpikeMovementType.Vertical)
        {
            direction *= -1;
        }
        else
        {
            currentDirIndex = (currentDirIndex + 1) % 4;
        }

        string hitStateName = GetHitAnimFromDirection(hitDirection);
        animator.Play(hitStateName, 0, 0f);
        Debug.Log($"▶️ animator.Play('{hitStateName}') called (wall dir: {hitDirection})");

        yield return new WaitForSeconds(0.3f);

        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        hitCooldownTimer = 0.8f;
        isWaiting = false;
    }

    float GetClipLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        return 0.3f;
    }

    void OnDrawGizmos()
    {
        if (col == null) return;

        Gizmos.color = Color.red;
        if (movementType == SpikeMovementType.Horizontal)
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(direction * obstacleCheckDistance, 0f, 0f));
        else if (movementType == SpikeMovementType.Vertical)
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0f, direction * obstacleCheckDistance, 0f));
        else
        {
            Vector2 dir = GetCurrentDirection();
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * obstacleCheckDistance);
        }

        Gizmos.color = Color.yellow;
        if (movementType == SpikeMovementType.Horizontal)
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(direction * crushCheckDistance, 0f, 0f));
        else if (movementType == SpikeMovementType.Vertical)
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0f, direction * crushCheckDistance, 0f));
        else
        {
            Vector2 dir = GetCurrentDirection();
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * crushCheckDistance);
        }
    }
}