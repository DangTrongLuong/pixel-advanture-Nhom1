using UnityEngine;
using System.Collections;

public class SpikeHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 16f;        // tăng từ 12 lên 16
    [SerializeField] private float hitAnimSpeed = 2.5f;
    [SerializeField] private float normalAnimSpeed = 1f;
    [SerializeField] private float waitAfterHit = 0.6f;    // tăng từ 0.15 lên 0.6
    [SerializeField] private float blinkInterval = 1.5f;

    [Header("Layer Check")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    private float obstacleCheckDistance;

    [Header("Direction")]
    [SerializeField] private int startDirection = 1;

    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D col;
    private int direction;
    private bool isWaiting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        direction = startDirection;

        if (col != null)
            obstacleCheckDistance = col.size.x / 2f + 0.05f;

        StartCoroutine(BlinkLoop());
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

        // Dính cứng vào tường
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // 🔥 CAMERA SHAKE ONLY
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.15f, 0.08f);
        }

        // Animation hit nhanh
        animator.speed = hitAnimSpeed;
        if (direction == 1)
            animator.SetTrigger("RightHit");
        else
            animator.SetTrigger("LeftHit");

        float clipName = direction == 1 ? 1 : -1;
        string triggerName = direction == 1 ? "RightHit" : "LeftHit";
        float clipLength = GetClipLength(triggerName);
        yield return new WaitForSeconds(clipLength / hitAnimSpeed);

        // Blink
        animator.SetTrigger("Blink");
        float blinkLength = GetClipLength("Blink");
        yield return new WaitForSeconds(blinkLength / hitAnimSpeed);

        // Dừng thêm 1 chút trước khi dội
        animator.speed = normalAnimSpeed;
        yield return new WaitForSeconds(waitAfterHit);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        direction *= -1;

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
        int dir = Application.isPlaying ? direction : startDirection;
        Gizmos.DrawLine(
            transform.position,
            transform.position + new Vector3(dir * obstacleCheckDistance, 0f, 0f)
        );
    }
}