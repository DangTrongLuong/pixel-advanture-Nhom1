using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Cần thư viện này để điều khiển UI Image
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Cài đặt thời gian")]
    [SerializeField] private float restartDelay = 2f; // Thời gian chờ nhân vật rơi trước khi load lại game

    [Header("Hiệu ứng Nháy Sáng")]
    [SerializeField] private Image flashImage; // Kéo cái FlashImage vào đây
    [SerializeField] private float flashSpeed = 10f; // Tốc độ nháy

    private bool hasTriggered = false; // Đảm bảo chỉ kích hoạt 1 lần

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu chưa kích hoạt và đối tượng là Player
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            // Bắt đầu chuỗi hiệu ứng chết
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    // Coroutine điều phối chính
    private IEnumerator DeathSequence(GameObject player)
    {
        // 1. Bắt đầu hiệu ứng nháy sáng (chạy song song)
        if (flashImage != null)
        {
            StartCoroutine(FlashScreenEffect());
        }

        // 2. Xử lý nhân vật rơi tự do
        Collider2D playerCol = player.GetComponent<Collider2D>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerCol != null && playerRb != null)
        {
            // Tắt Collider để nhân vật đi xuyên qua mọi thứ (rơi khỏi map)
            playerCol.enabled = false;
            
            // Đảm bảo Rigidbody là Dynamic để trọng lực kéo xuống
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            
            // Tùy chọn: Thêm một lực đẩy nhẹ lên trên trước khi rơi cho kịch tính
            playerRb.linearVelocity = Vector2.zero; // Reset vận tốc cũ
            playerRb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }

        // 3. Chờ đợi trong khi nhân vật rơi
        yield return new WaitForSeconds(restartDelay);

        // 4. Load lại màn chơi
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Coroutine xử lý hiệu ứng làm mờ/hiện rõ tấm ảnh trắng
    private IEnumerator FlashScreenEffect()
    {
        float alpha = 0;
        // Làm sáng lên (Fade in)
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * flashSpeed;
            SetFlashAlpha(alpha);
            yield return null; // Chờ đến frame tiếp theo
        }

        // Làm mờ đi (Fade out)
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * flashSpeed;
            SetFlashAlpha(alpha);
            yield return null;
        }
        SetFlashAlpha(0); // Đảm bảo về 0 khi kết thúc
    }

    // Hàm phụ trợ để set độ trong suốt cho ảnh
    private void SetFlashAlpha(float alpha)
    {
        if (flashImage == null) return;
        Color color = flashImage.color;
        color.a = Mathf.Clamp01(alpha); // Giới hạn alpha từ 0 đến 1
        flashImage.color = color;
    }
}