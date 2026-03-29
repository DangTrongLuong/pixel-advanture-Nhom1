using UnityEngine;
using System.Collections;

public class SpikeHeadVertical : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 0.6f;
    [SerializeField] private float blinkInterval = 1.5f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    private float obstacleCheckDistance;

    [Header("Direction")]
    [SerializeField] private int startDirection = 0;

    private readonly Vector2[] directions = {
        Vector2.up,
        Vector2.down
    };

    private readonly string[] hitTriggers = {
        "TopHit",
        "BottomHit"
    };

    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D col;
    private int currentDirIndex;
    private bool isWaiting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        currentDirIndex = startDirection;

        if (col != null)
            obstacleCheckDistance = col.size.y / 2f + 0.05f;

        StartCoroutine(BlinkLoop());
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
        {
            StartCoroutine(Bounce());
        }
    }

    IEnumerator Bounce()
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

        // Dừng thêm trước khi dội
        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentDirIndex = (currentDirIndex + 1) % 2;

        isWaiting = false;
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);

            if (!isWaiting)
                animator.SetTrigger("Blink");
        }
    }

    float GetClipLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0.3f;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        Vector2 knockDir = (other.transform.position - transform.position).normalized;
        other.gameObject.GetComponent<PlayerController>()
            ?.TakeDamage(knockDir.x * 12f, knockDir.y * 8f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        int dir = Application.isPlaying ? currentDirIndex : startDirection;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)(directions[dir] * obstacleCheckDistance)
        );
    }
}