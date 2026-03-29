using UnityEngine;
using System.Collections;

public class RockHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 0.6f;

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
        rb.linearVelocity = new Vector2(direction * moveSpeed, 0f);
        CheckObstacle();
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

        // Dùng DotTest — player nhảy lên đầu thì không chết
        Vector2 dirToPlayer = other.transform.position - transform.position;
        bool playerOnTop = Vector2.Dot(dirToPlayer.normalized, Vector2.up) > 0.5f;
        if (playerOnTop) return;

        LayerMask obstacleLayer = wallLayer | groundLayer;
        Vector2 origin = new Vector2(
            other.transform.position.x,
            other.transform.position.y
        );

        RaycastHit2D wallBehindPlayer = Physics2D.Raycast(
            origin,
            new Vector2(direction, 0f),
            crushCheckDistance,
            obstacleLayer
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