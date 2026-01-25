using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [Header("Cài đặt thời gian & Hiệu ứng")]
    [SerializeField] private float fallDelay = 2f; // Thời gian chờ trước khi rơi
    [SerializeField] private float shakeMagnitude = 0.1f; // Độ mạnh của rung lắc
    [SerializeField] private float shakeSpeed = 50f; // Tốc độ rung lắc

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isShaking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        // Đảm bảo ban đầu ván không bị rơi
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    void Update()
    {
        if (isShaking)
        {
            // Tạo hiệu ứng rung lắc nhẹ quanh vị trí gốc
            float newX = startPos.x + Mathf.Sin(Time.time * shakeSpeed) * shakeMagnitude;
            transform.position = new Vector2(newX, transform.position.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu nhân vật đứng lên trên tấm ván
        if (collision.gameObject.CompareTag("Player") && collision.GetContact(0).normal.y < -0.5f)
        {
            StartCoroutine(FallSequence(collision.transform));
        }
    }

    private IEnumerator FallSequence(Transform playerTransform)
    {
        isShaking = true;

        // Chờ 2 giây như yêu cầu
        yield return new WaitForSeconds(fallDelay);

        isShaking = false;
        // Chuyển sang Dynamic để rơi theo trọng lực
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Tách nhân vật ra để không bị rơi theo ván (nếu đang là con của ván)
        if (playerTransform.parent == transform)
        {
            playerTransform.SetParent(null);
        }

        // Tùy chọn: Xóa ván sau khi rơi 3 giây để tránh rác bộ nhớ
        Destroy(gameObject, 3f);
    }
}