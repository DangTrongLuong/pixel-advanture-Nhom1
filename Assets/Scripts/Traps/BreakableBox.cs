using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [Header("Cài đặt Items rơi ra")]
    [Tooltip("Kéo các prefab hoa quả vào đây")]
    [SerializeField] private GameObject[] itemPrefabs; 

    [Header("Lực bắn ra")]
    [SerializeField] private float forceX = 3f; // Lực bắn sang ngang
    [SerializeField] private float forceY = 5f; // Lực nảy lên cao

    [Header("Cài đặt Animation")]
    [Tooltip("Thời gian chờ animation vỡ chạy xong trước khi xóa hộp")]
    [SerializeField] private float destroyDelay = 0.5f; 

    private Animator anim;
    private Collider2D col;
    private bool isBroken = false; // Đảm bảo chỉ vỡ 1 lần

    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Kiểm tra điều kiện: Chưa vỡ VÀ người chạm là Player
        // (Đảm bảo nhân vật có Tag "Player" như các bài trước)
        if (!isBroken && collision.gameObject.CompareTag("Player"))
        {
            // Mẹo: Nếu chỉ muốn vỡ khi nhảy đụng đầu từ dưới lên (kiểu Mario)
            // Hãy mở comment dòng dưới và đóng comment dòng if ở trên lại
            // if (!isBroken && collision.gameObject.CompareTag("Player") && collision.GetContact(0).normal.y > 0.5f)
            {
                BreakTheBox();
            }
        }
    }

    public void BreakTheBox()
    {
        isBroken = true;

        // 2. Tắt Collider ngay lập tức để nhân vật không bị kẹt khi hộp đang vỡ
        col.enabled = false;

        // 3. Chạy animation vỡ
        anim.SetTrigger("doBreak");

        // 4. Sinh ra items
        SpawnItems();

        // 5. Phá hủy hộp sau khi animation chạy xong
        // (Hãy chỉnh destroyDelay khớp với độ dài animation vỡ của bạn)
        Destroy(gameObject, destroyDelay);
    }

    private void SpawnItems()
    {
        if (itemPrefabs.Length == 0) return;

        // Bắn 1 quả sang trái
        SpawnSingleItem(Vector2.left);
        // Bắn 1 quả sang phải
        SpawnSingleItem(Vector2.right);
    }

    private void SpawnSingleItem(Vector2 direction)
    {
        // Chọn ngẫu nhiên 1 loại quả từ danh sách
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedPrefab = itemPrefabs[randomIndex];

        // Vị trí sinh ra (cao hơn tâm hộp 1 chút)
        Vector2 spawnPos = transform.position + Vector3.up * 0.5f;
        
        // Tạo item
        GameObject item = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        // Áp dụng lực bắn
        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        if (itemRb != null)
        {
            // Tạo vector lực chéo lên (sang bên + lên trên)
            Vector2 force = new Vector2(direction.x * forceX, forceY);
            itemRb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}