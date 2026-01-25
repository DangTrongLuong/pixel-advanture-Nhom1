using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Thiết lập di chuyển")]
    [SerializeField] private Vector2 moveOffset; // Khoảng cách di chuyển (ví dụ: X=5 nếu muốn đi ngang 5m)
    [SerializeField] private float speed = 2f;

    private Vector2 startPos;
    private Vector2 targetPos;
    private bool isPlayerOn = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
    }

    void Update()
    {
        // Xác định vị trí cần đến dựa trên việc nhân vật có đang đứng trên ván hay không
        Vector2 currentDestination = isPlayerOn ? targetPos : startPos;

        // Di chuyển tấm ván mượt mà
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);
    }

    // Kiểm tra khi nhân vật chạm vào tấm ván
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Kiểm tra xem điểm va chạm có phải từ phía trên không
        // Normal.y < -0.5f có nghĩa là vật thể va chạm (player) đang nằm ở phía trên tấm ván
        if (collision.GetContact(0).normal.y < -0.5f)
        {
            isPlayerOn = true;
            collision.transform.SetParent(transform);
        }
        }
    }

    // Kiểm tra khi nhân vật rời khỏi tấm ván
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
            // Bỏ nhân vật ra khỏi làm con của tấm ván
            collision.transform.SetParent(null);
        }
    }
}