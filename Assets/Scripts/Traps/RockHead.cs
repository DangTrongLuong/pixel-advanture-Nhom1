using UnityEngine;
using System.Collections;

public class RockHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 1.2f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    private float wallCheckDistance;
    private float crushCheckDistance;

    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int direction = 1;
    private bool isWaiting = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        if (col != null)
        {
            wallCheckDistance = col.size.x / 2f + 0.05f;
            crushCheckDistance = col.size.x / 2f + 0.3f;
        }
    }

    void Update()
    {
        if (isWaiting) return;
        
        float targetVelocity = direction * moveSpeed;
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVelocity, Time.deltaTime * 5f), 0f);
        DetectAndMovePlayerOnTop();
        CheckObstacle();
    }

    private void DetectAndMovePlayerOnTop()
    {
        // Use OverlapBox to detect player standing on top
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
                    // KHÔNG sync nếu player rơi xuống (Y velocity âm)
                    if (playerRb.linearVelocity.y < -0.1f) return;
                    
                    playerRb.linearVelocity = new Vector2(rb.linearVelocity.x, playerRb.linearVelocity.y);
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

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            new Vector2(direction, 0f),
            wallCheckDistance,
            obstacleLayer
        );

        if (hit.collider != null && !isWaiting)
            StartCoroutine(HitWall());
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (isWaiting) return;

        // Check xem player có đứng trên TOP của RockHead không
        float rockHeadTopY = transform.position.y + col.size.y / 2f;
        bool playerOnTop = other.transform.position.y > rockHeadTopY;
        
        if (playerOnTop)
        {
            // Nếu đứng trên đầu, check xem RockHead có hit obstacle không
            // Nếu có + RockHead đẩy lên/sang → player bị đẹ lại → chết
            LayerMask obstacleLayer = wallLayer | groundLayer;
            RaycastHit2D obstacleAhead = Physics2D.Raycast(
                transform.position + (Vector3)Vector2.up * col.size.y / 2f,
                Vector2.up,
                1.0f,
                obstacleLayer
            );
            
            if (obstacleAhead.collider != null)
            {
                // RockHead hit ceiling → player chết
                other.gameObject.GetComponent<PlayerController>()
                    ?.TakeDamage(0f, 10f);
            }
            return;
        }

        // Chỉ check crush nếu player NOT ở trên top (bị dính cạnh)
        LayerMask obstacleLayer2 = wallLayer | groundLayer;
        Vector2 origin = new Vector2(
            other.transform.position.x,
            other.transform.position.y
        );

        RaycastHit2D wallBehindPlayer = Physics2D.Raycast(
            origin,
            new Vector2(direction, 0f),
            crushCheckDistance,
            obstacleLayer2
        );

        if (wallBehindPlayer.collider != null)
        {
            float knockDir = -direction;
            other.gameObject.GetComponent<PlayerController>()
                ?.TakeDamage(knockDir * 12f, 10f);
        }
    }

    IEnumerator HitWall()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.speed = hitAnimSpeed;
        animator.SetTrigger("Blink");

        float blinkLength = GetClipLength("Blink");
        yield return new WaitForSeconds(blinkLength / hitAnimSpeed);

        direction *= -1;

        if (direction == 1)
            animator.SetTrigger("RightHit");
        else
            animator.SetTrigger("LeftHit");

        float hitLength = GetClipLength(direction == 1 ? "RightHit" : "LeftHit");
        yield return new WaitForSeconds(hitLength / hitAnimSpeed);

        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,
            transform.position + new Vector3(direction * wallCheckDistance, 0f, 0f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,
            transform.position + new Vector3(direction * crushCheckDistance, 0f, 0f));
    }
}