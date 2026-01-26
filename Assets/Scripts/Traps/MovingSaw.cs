using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MovingSaw : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    [SerializeField] private Vector2[] waypoints; // 4 điểm của hình vuông
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Hiệu ứng Cái chết")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashSpeed = 10f;
    [SerializeField] private float restartDelay = 2f;

    private int currentWaypointIndex = 0;
    private bool isPlayerDead = false;

    void Update()
    {
        if (isPlayerDead) return;

        MoveAlongPath();
        RotateSaw();
    }

    private void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;

        // Di chuyển tới điểm đích hiện tại
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex], moveSpeed * Time.deltaTime);

        // Nếu đã đến đích, chuyển sang điểm tiếp theo
        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void RotateSaw()
    {
        // Xoay lưỡi cưa liên tục quanh trục Z
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra Tag Player dựa trên quản lý của PersistentPlayerManager
        if (!isPlayerDead && collision.CompareTag("Player"))
        {
            isPlayerDead = true;
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    private IEnumerator DeathSequence(GameObject player)
    {
        // 1. Nháy sáng màn hình
        if (flashImage != null) StartCoroutine(FlashEffect());

        // 2. Vô hiệu hóa va chạm và cho nhân vật rơi tự do
        Collider2D col = player.GetComponent<Collider2D>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (col != null) col.enabled = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(restartDelay);

        // 3. Load lại màn chơi - PersistentPlayerManager sẽ tự cập nhật lại nhân vật
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FlashEffect()
    {
        float alpha = 0;
        while (alpha < 1f) {
            alpha += Time.deltaTime * flashSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        while (alpha > 0f) {
            alpha -= Time.deltaTime * flashSpeed;
            SetAlpha(alpha);
            yield return null;
        }
    }

    private void SetAlpha(float a) {
        Color c = flashImage.color;
        c.a = a;
        flashImage.color = c;
    }
}