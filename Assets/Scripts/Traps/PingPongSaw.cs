using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PingPongSaw : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    [SerializeField] private Vector2 moveOffset; // Khoảng cách di chuyển từ vị trí gốc
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 300f;

    [Header("Hiệu ứng Cái chết")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashSpeed = 10f;
    [SerializeField] private float restartDelay = 2f;

    private Vector2 startPos;
    private Vector2 targetPos;
    private bool isPlayerDead = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
    }

    void Update()
    {
        if (isPlayerDead) return;

        // Di chuyển qua lại mượt mà giữa Start và Target
        float time = Mathf.PingPong(Time.time * speed, 1);
        transform.position = Vector2.Lerp(startPos, targetPos, time);

        // Xoay lưỡi cưa
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra Tag Player (đảm bảo nhân vật từ PersistentPlayerManager có gắn Tag này)
        if (!isPlayerDead && collision.CompareTag("Player"))
        {
            isPlayerDead = true;
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    private IEnumerator DeathSequence(GameObject player)
    {
        // 1. Nháy sáng màn hình
        if (flashImage != null) StartCoroutine(FlashScreen());

        // 2. Vô hiệu hóa va chạm và cho nhân vật rơi tự do
        Collider2D col = player.GetComponent<Collider2D>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (col != null) col.enabled = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero; // Triệt tiêu vận tốc cũ
            rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); // Đẩy nhẹ lên trước khi rơi
        }

        yield return new WaitForSeconds(restartDelay);

        // 3. Load lại màn chơi
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FlashScreen()
    {
        float alpha = 0;
        while (alpha < 1f) {
            alpha += Time.deltaTime * flashSpeed;
            SetFlashAlpha(alpha);
            yield return null;
        }
        while (alpha > 0f) {
            alpha -= Time.deltaTime * flashSpeed;
            SetFlashAlpha(alpha);
            yield return null;
        }
    }

    private void SetFlashAlpha(float a) {
        if (flashImage == null) return;
        Color c = flashImage.color;
        c.a = a;
        flashImage.color = c;
    }
}