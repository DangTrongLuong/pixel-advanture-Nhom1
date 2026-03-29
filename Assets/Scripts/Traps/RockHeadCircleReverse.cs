using UnityEngine;
using System.Collections;

public class RockHeadCircleReverse : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 0.6f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    private float obstacleCheckDistance;
    private float crushCheckDistance;

    private readonly Vector2[] directions = {
        Vector2.down, Vector2.left, Vector2.up, Vector2.right
    };

    private readonly string[] hitTriggers = {
        "BottomHit", "LeftHit", "TopHit", "RightHit"
    };

    [SerializeField] private int startDirection = 0;
    private int currentDirIndex;
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isWaiting = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        currentDirIndex = startDirection;

        if (col != null)
        {
            obstacleCheckDistance = col.size.x / 2f + 0.05f;
            crushCheckDistance = col.size.x / 2f + 0.3f;
        }
    }

    void Update()
    {
        if (isWaiting) return;
        rb.linearVelocity = directions[currentDirIndex] * moveSpeed;
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
            directions[currentDirIndex],
            obstacleCheckDistance,
            obstacleLayer
        );

        if (hit.collider != null && !isWaiting)
            StartCoroutine(HitObstacle());
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (isWaiting) return;

        // Player nhảy lên đầu không chết
        Vector2 dirToPlayer = other.transform.position - transform.position;
        bool playerOnTop = Vector2.Dot(dirToPlayer.normalized, Vector2.up) > 0.5f;
        if (playerOnTop) return;

        LayerMask obstacleLayer = wallLayer | groundLayer;

        RaycastHit2D obstacleBehind = Physics2D.Raycast(
            other.transform.position,
            directions[currentDirIndex],
            crushCheckDistance,
            obstacleLayer
        );

        if (obstacleBehind.collider != null)
        {
            Vector2 knockback = -directions[currentDirIndex] * 12f;
            other.gameObject.GetComponent<PlayerController>()
                ?.TakeDamage(knockback.x, knockback.y);
        }
    }

    IEnumerator HitObstacle()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.speed = hitAnimSpeed;
        animator.SetTrigger(hitTriggers[currentDirIndex]);

        float clipLength = GetClipLength(hitTriggers[currentDirIndex]);
        yield return new WaitForSeconds(clipLength / hitAnimSpeed);

        animator.SetTrigger("Blink");
        float blinkLength = GetClipLength("Blink");
        yield return new WaitForSeconds(blinkLength / hitAnimSpeed);

        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentDirIndex = (currentDirIndex + 1) % 4;
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
        Vector2 currentDir = Application.isPlaying
            ? directions[currentDirIndex] : directions[startDirection];
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)(currentDir * obstacleCheckDistance));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)(currentDir * crushCheckDistance));
    }
}