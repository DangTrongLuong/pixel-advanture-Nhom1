using UnityEngine;
using System.Collections;

public class RockHeadVertical : MonoBehaviour
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

    [SerializeField] private int startDirection = -1; // -1=xuống, 1=lên

    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int direction;
    private bool isWaiting = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        direction = startDirection;

        if (col != null)
        {
            obstacleCheckDistance = col.size.y / 2f + 0.05f;
            crushCheckDistance = col.size.y / 2f + 0.3f;
        }
    }

    void Update()
    {
        if (isWaiting) return;
        rb.linearVelocity = new Vector2(0f, direction * moveSpeed);
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
            new Vector2(0f, direction),
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
            new Vector2(0f, direction),
            crushCheckDistance,
            obstacleLayer
        );

        if (obstacleBehind.collider != null)
        {
            float knockY = -direction * 12f;
            other.gameObject.GetComponent<PlayerController>()
                ?.TakeDamage(0f, knockY);
        }
    }

    IEnumerator HitObstacle()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.speed = hitAnimSpeed;

        string triggerName = direction == -1 ? "BottomHit" : "TopHit";
        animator.SetTrigger(triggerName);

        float clipLength = GetClipLength(triggerName);
        yield return new WaitForSeconds(clipLength / hitAnimSpeed);

        animator.SetTrigger("Blink");
        float blinkLength = GetClipLength("Blink");
        yield return new WaitForSeconds(blinkLength / hitAnimSpeed);

        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        direction *= -1;
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
            transform.position + new Vector3(0f, direction * obstacleCheckDistance, 0f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,
            transform.position + new Vector3(0f, direction * crushCheckDistance, 0f));
    }
}