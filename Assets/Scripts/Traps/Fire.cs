using UnityEngine;

public class Fire : MonoBehaviour
{
    [Header("Damage Zone")]
    [SerializeField] private Vector2 damageOffsetBase = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 damageSize = new Vector2(0.8f, 1f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float knockbackForceX = 5f;  // Knockback X direction

    private Animator animator;
    private bool triggered = false;
    private bool active = false;
    private LayerMask trapLayer;  // Để detect player đứng trên trap

    private static readonly int activateAnim = Animator.StringToHash("Activate");

    // ─── Tính damageOffset dựa vào rotation ────────────────────────
    private Vector2 GetDamageOffset()
    {
        // Lấy rotation Z hiện tại (chuẩn hóa 0-360)
        float rotZ = transform.eulerAngles.z;
        
        // Normalize: -90° → 270°, -270° → 90°
        if (rotZ > 180f) rotZ -= 360f;

        // Xác định hướng và quay offset
        if (Mathf.Abs(rotZ - (-90f)) < 45f || Mathf.Abs(rotZ - 270f) < 45f)
        {
            // Quay -90°: lửa bắn phải → offset = (1, 0)
            return new Vector2(damageOffsetBase.y, 0f);
        }
        else if (Mathf.Abs(rotZ - (-270f)) < 45f || Mathf.Abs(rotZ - 90f) < 45f)
        {
            // Quay -270° (hay 90°): lửa bắn trái → offset = (-1, 0)
            return new Vector2(-damageOffsetBase.y, 0f);
        }
        else
        {
            // Mặc định: trên → offset = (0, 1)
            return damageOffsetBase;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        trapLayer = LayerMask.GetMask("Traps");
    }

    void Start()
    {
        animator.Play("Trap_Off", 0, 0f);
    }

    // ─── Player chạm vào trap → kích hoạt ───────────────────────────
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            // Kiểm tra xem player có đang từ phía TRÊN trap không
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            float fireTopY = transform.position.y + col.size.y / 2f;
            bool playerFromTop = other.transform.position.y > fireTopY;
            
            // Chỉ kích hoạt nếu player từ phía trên
            if (!playerFromTop) return;
            
            triggered = true;
            animator.ResetTrigger(activateAnim);
            animator.SetTrigger(activateAnim);
            Debug.Log("[Fire] Triggered by player — playing Trap_Hit");
        }
    }

    // Fallback: trigger via OnTriggerEnter2D nếu Fire trap có collider trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            // Kiểm tra xem player có đang từ phía TRÊN trap không
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            float fireTopY = transform.position.y + col.size.y / 2f;
            bool playerFromTop = other.transform.position.y > fireTopY;
            
            // Chỉ kích hoạt nếu player từ phía trên
            if (!playerFromTop) return;
            
            triggered = true;
            animator.ResetTrigger(activateAnim);
            animator.SetTrigger(activateAnim);
            Debug.Log("[Fire] Triggered by player (trigger) — playing Trap_Hit");
        }
    }

    // ─── GỌI TỪ ANIMATION EVENT: frame lửa bật (Trap_On) ────────────
    public void ActivateDamage()
    {
        // Safety check: chỉ gây damage nếu trap đã được trigger bởi player
        if (!triggered) return;

        active = true;
        Vector2 offset = GetDamageOffset();

        // Detect player trên playerLayer OR trapLayer (để detect player đứng trên trap)
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + offset,
            damageSize,
            0f,
            playerLayer | trapLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var pc = hit.GetComponent<PlayerController>();
                if (pc != null)
                {
                    // Gây damage với knockback direction để tạo hiệu ứng chết giống Map 10
                    pc.TakeDamage(knockbackForceX, 0f);
                }
                break;
            }
        }

        // Fallback: nếu không tìm thấy player qua OverlapBox, dùng FindWithTag
        if (hits.Length == 0)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 center = (Vector2)transform.position + offset;
                Vector2 playerPos = player.transform.position;
                float dx = Mathf.Abs(playerPos.x - center.x);
                float dy = Mathf.Abs(playerPos.y - center.y);

                if (dx <= damageSize.x * 0.5f && dy <= damageSize.y * 0.5f)
                {
                    player.GetComponent<PlayerController>()?.TakeDamage(knockbackForceX, 0f);
                }
            }
        }
    }

    // ─── GỌI TỪ ANIMATION EVENT: frame lửa tắt (Trap_Off) ──────────
    public void DeactivateDamage()
    {
        active = false;
        triggered = false;
    }

    // ─── Update: backup liên tục khi player đứng trong lửa ──────────
    // OnCollisionStay2D không đáng tin do Unity sleep optimization
    void Update()
    {
        if (!active) return;

        Vector2 offset = GetDamageOffset();
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + offset,
            damageSize,
            0f,
            playerLayer | trapLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>()?.TakeDamage(knockbackForceX, 0f);
                break;
            }
        }
    }

    // ─── Debug Gizmo ─────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        Vector2 offset = GetDamageOffset();
        Gizmos.color = active ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)offset,
            damageSize
        );
    }
}