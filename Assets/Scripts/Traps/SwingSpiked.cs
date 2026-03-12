using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class SwingSpiked : MonoBehaviour
{
    // Thêm Static vào danh sách các chế độ
    public enum RotationMode { Pendulum180, FullRotation360, Static }

    [Header("Cấu hình Chế độ")]
    [SerializeField] private RotationMode mode = RotationMode.Pendulum180;
    [SerializeField] private float speed = 2f;      
    [SerializeField] private float maxAngle = 90f;  

    [Header("Hiệu ứng Cái chết")]
    [SerializeField] private Image flashImage;      
    [SerializeField] private float restartDelay = 2f;

    private bool isPlayerDead = false;

    void Update()
    {
        if (isPlayerDead) return;

        // Nếu ở chế độ Static, chúng ta thoát hàm Update sớm, không xoay gì cả
        if (mode == RotationMode.Static) return;

        float angle = 0f;

        if (mode == RotationMode.Pendulum180)
        {
            angle = Mathf.Sin(Time.time * speed) * maxAngle;
        }
        else if (mode == RotationMode.FullRotation360)
        {
            angle = Time.time * speed * -100f;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Logic va chạm vẫn giữ nguyên để đảm bảo nhân vật chết khi chạm vào
        if (!isPlayerDead && collision.CompareTag("Player"))
        {
            isPlayerDead = true;
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    private IEnumerator DeathSequence(GameObject player)
    {
        if (flashImage != null) StartCoroutine(FlashEffect());

        Collider2D col = player.GetComponent<Collider2D>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (col != null) col.enabled = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * 8f, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FlashEffect()
    {
        float alpha = 0;
        while (alpha < 1f) {
            alpha += Time.deltaTime * 15f;
            SetAlpha(alpha);
            yield return null;
        }
        while (alpha > 0f) {
            alpha -= Time.deltaTime * 5f;
            SetAlpha(alpha);
            yield return null;
        }
    }

    private void SetAlpha(float a) {
        if (flashImage == null) return;
        Color c = flashImage.color;
        c.a = a;
        flashImage.color = c;
    }
}
