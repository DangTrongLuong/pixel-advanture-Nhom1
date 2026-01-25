using UnityEngine;

public class ContinuousMovingPlatform : MonoBehaviour
{
    [Header("Thiết lập di chuyển")]
    [SerializeField] private Vector2 moveDirection; // Hướng và khoảng cách (ví dụ: X=5 là đi ngang 5m)
    [SerializeField] private float speed = 2f;

    private Vector2 startPos;
    private Vector2 targetPos;

    void Start()
    {
        startPos = transform.position;
        // Điểm đích bằng vị trí bắt đầu cộng với khoảng cách thiết lập
        targetPos = startPos + moveDirection;
    }

    void Update()
    {
        // Tính toán tỷ lệ di chuyển dựa trên thời gian
        float pingPong = Mathf.PingPong(Time.time * speed, 1);
        
        // Di chuyển tấm ván giữa Start và Target
        transform.position = Vector2.Lerp(startPos, targetPos, pingPong);
    }

    // Giữ nhân vật đứng yên trên ván khi ván di chuyển
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
