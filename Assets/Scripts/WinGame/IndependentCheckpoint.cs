using UnityEngine;
using System.Collections;

public class IndependentCheckpoint : MonoBehaviour
{
    public enum CheckpointType { Intermediate, Finish }

    [Header("Cấu hình Loại")]
    [SerializeField] private CheckpointType type = CheckpointType.Intermediate;

    [Header("Kết nối Animator")]
    [SerializeField] private Animator visualAnimator; // Animator của Lá cờ/Cúp
    [SerializeField] private string triggerName = "Activate";

    [Header("Cấu hình Camera Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    [Header("Hệ thống Pháo hoa")]
    [SerializeField] private ParticleSystem[] fireworkParticles;

    [Header("Hiệu ứng Về đích (Animation)")]
    [SerializeField] private float bounceForce = 12f; // Lực nảy lên khi chạm cúp
    [SerializeField] private float disappearDelay = 0.5f; // Thời gian bay lên trước khi nổ

    private bool isReached = false; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra đúng nhân vật
        if (!isReached && collision.CompareTag("Player"))
        {
            // Kiểm tra nếu nhân vật chạm từ trên xuống (giẫm lên cúp)
            bool isStomping = collision.transform.position.y > transform.position.y + 0.2f;

            if (type == CheckpointType.Finish && isStomping)
            {
                StartCoroutine(ExecuteFinishSequence(collision.gameObject));
            }
            else
            {
                Reached();
            }
        }
    }

    private void Reached()
    {
        isReached = true;

        if (visualAnimator != null) visualAnimator.SetTrigger(triggerName);

        // Kích hoạt pháo giấy rơi rụng xung quanh bục
        if (fireworkParticles != null)
        {
            foreach (ParticleSystem ps in fireworkParticles)
            {
                if (ps != null) { ps.Clear(); ps.Play(); }
            }
        }

        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);
        }
    }

    private IEnumerator ExecuteFinishSequence(GameObject player)
{
    isReached = true;
    Reached();

    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
    PlayerController pc = player.GetComponent<PlayerController>();
    Animator playerAnimator = player.GetComponent<Animator>();

    if (rb != null && pc != null)
    {
        // 1. Khóa điều khiển
        pc.enabled = false;

        // 2. Nảy lên
        rb.linearVelocity = new Vector2(0f, bounceForce);
        Debug.Log("Đang nảy lên...");
    }

    // 3. Chờ bay lên
    yield return new WaitForSeconds(disappearDelay);

    // 4. Kích hoạt animation nổ
    if (playerAnimator != null)
    {
        playerAnimator.SetBool("isExplode", true);
        Debug.Log("BÙM!");

        // Đợi chuyển sang state Explode
        yield return new WaitUntil(() =>
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Explode")
        );

        // Đợi animation chạy xong
        yield return new WaitForSeconds(
            playerAnimator.GetCurrentAnimatorStateInfo(0).length
        );
    }

    // 5. Ẩn player sau khi animation xong
    SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
    if (sr != null) sr.enabled = false;

    if (rb != null) rb.bodyType = RigidbodyType2D.Static;

    Debug.Log("FINISHED: Boom!");
}
}