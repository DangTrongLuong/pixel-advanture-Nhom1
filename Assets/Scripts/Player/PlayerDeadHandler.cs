using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Physics")]
    [SerializeField] private float bounceUpForce = 8f;
    [SerializeField] private float bounceBackForce = 4f;

    [Header("Flash Effect")]
    [SerializeField] private float flashDuration = 0.02f;
    [SerializeField] private int flashCount = 1;

    private Image flashImage;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private bool isDead = false;

    static readonly int AnimDead = Animator.StringToHash("isDead");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();

        var flashObj = GameObject.Find("DeathFlash");
        if (flashObj != null)
            flashImage = flashObj.GetComponent<Image>();
        else
            Debug.LogWarning("Không tìm thấy DeathFlash trong Scene!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Traps")) TriggerDeath();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Traps")) TriggerDeath();
    }

    private void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DieRoutine());
    }

    public void Die() => TriggerDeath();

    private IEnumerator DieRoutine()
    {
        playerController.enabled = false;
        animator.SetBool(AnimDead, true);

        spriteRenderer.sortingLayerName = "Foreground";
        spriteRenderer.sortingOrder = 999;

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        rb.simulated = true;

        float dir = transform.localScale.x;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 3f;
        rb.AddForce(new Vector2(-dir * bounceBackForce, bounceUpForce), ForceMode2D.Impulse);

        StartCoroutine(FlashScreen());

        yield return null;
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        Sprite lastSprite = GetLastSpriteFromClip("PlayerHit");
        animator.enabled = false;
        if (lastSprite != null)
            spriteRenderer.sprite = lastSprite;
    }

    private Sprite GetLastSpriteFromClip(string clipName)
    {
        // Logic cũ sử dụng UnityEditor API - không thể chạy trong game
        // Thay vào đó, lấy sprite current từ SpriteRenderer
        return spriteRenderer.sprite;
    }

    private IEnumerator FlashScreen()
    {
        if (flashImage == null) yield break;
        for (int i = 0; i < flashCount; i++)
        {
            flashImage.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(flashDuration);
            flashImage.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(flashDuration);
        }
        flashImage.color = new Color(1f, 1f, 1f, 0f);
    }
}