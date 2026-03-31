using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    private AudioManager audioManager;

    private void Awake() 
    {
        // Tìm các Manager trong Scene
        gameManager = Object.FindFirstObjectByType<GameManager>();
        audioManager = Object.FindFirstObjectByType<AudioManager>();
    }

    // Hàm này tự động được Unity gọi khi có va chạm với 1 Trigger Collider
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // 1. Kiểm tra Tag của vật thể va chạm
        if (collision.CompareTag("Items")) 
        {
            // 2. Lấy script bảo vệ từ item (nếu có)
            ItemSpawnProtection protection = collision.GetComponent<ItemSpawnProtection>();

            // 3. Chỉ nhặt nếu item không có script bảo vệ HOẶC đã hết thời gian bảo vệ
            if (protection == null || protection.canBePickedUp) 
            {
                CollectItem(collision.gameObject);
            }
        }
    }

    private void CollectItem(GameObject itemObj) 
    {
        // Cộng điểm và phát âm thanh
        if (gameManager != null) gameManager.AddScore(1);
        if (audioManager != null) audioManager.PlayCoinSound();

        // Xóa item khỏi Scene
        Destroy(itemObj);
        Debug.Log("Đã nhặt Item bằng sự kiện OnTriggerEnter2D!");
    }
}