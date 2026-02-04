using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Cài đặt quét Item")]
    [SerializeField] private float detectionRadius = 0.5f; // Bán kính quét
    [SerializeField] private LayerMask itemLayer; // Chỉ định Layer chứa hoa quả

    private GameManager gameManager;
    private AudioManager audioManager;

    private void Awake() {
        gameManager = Object.FindFirstObjectByType<GameManager>();
        audioManager = Object.FindFirstObjectByType<AudioManager>();
    }

    private void Update() {
        // Quét các vật thể ở Layer Items
    Collider2D item = Physics2D.OverlapCircle(transform.position, detectionRadius, itemLayer);

    if (item != null) {
        // Lấy script bảo vệ từ item vừa quét được
        ItemSpawnProtection protection = item.GetComponent<ItemSpawnProtection>();

        // Chỉ nhặt nếu item không có script bảo vệ HOẶC đã hết thời gian bảo vệ
        if (protection == null || protection.canBePickedUp) {
            if (item.CompareTag("Items")) {
                CollectItem(item.gameObject);
            }
        }
    }
    }

    private void CollectItem(GameObject itemObj) {
        // Thực hiện các lệnh từ GameManager và AudioManager
        if (gameManager != null) gameManager.AddScore(1);
        if (audioManager != null) audioManager.PlayCoinSound();

        Destroy(itemObj);
        Debug.Log("Đã nhặt Item bằng phương pháp OverlapCircle!");
    }

    // Vẽ vòng tròn quét trong Scene để bạn dễ căn chỉnh bán kính
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}