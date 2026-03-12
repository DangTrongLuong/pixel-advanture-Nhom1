using UnityEngine;
using System.Collections;

public class AdvancedBreakableBox : MonoBehaviour
{
    public enum BoxType { Breakable, MultiHit, Persistent }

    [Header("Cấu hình Loại Hộp")]
    [SerializeField] private BoxType type = BoxType.MultiHit;
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Sprite emptyBoxSprite; // Dùng cho loại Persistent

    [Header("Cấu hình Vật phẩm")]
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private int itemsPerHit = 1;
    [SerializeField] private float launchForceX = 4f;
    [SerializeField] private float launchForceY = 6f;

    [Header("Hiệu ứng & Animation")]
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private string hitTrigger = "doHit";
    [SerializeField] private string breakTrigger = "doBreak";

    private int currentHits;
    private bool isDepleted = false;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        currentHits = maxHits;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra Tag Player và hướng va chạm từ dưới lên
        if (!isDepleted && collision.gameObject.CompareTag("Player") && collision.GetContact(0).normal.y > 0.5f)
        {
            HandleHit();
        }
    }

    private void HandleHit()
    {
        currentHits--;

        // 1. Hiệu ứng nảy/rung khi chạm
        if (anim != null) anim.SetTrigger(hitTrigger);

        // 2. Rơi item (nếu là loại MultiHit hoặc còn lượt)
        if (type == BoxType.MultiHit || (type == BoxType.Persistent && currentHits >= 0))
        {
            SpawnItems();
        }

        // 3. Kiểm tra trạng thái kết thúc
        if (currentHits <= 0)
        {
            FinalizeBox();
        }
    }

    private void FinalizeBox()
    {
        isDepleted = true;

        if (type == BoxType.Breakable)
        {
            col.enabled = false;
            if (anim != null) anim.SetTrigger(breakTrigger);
            Destroy(gameObject, destroyDelay);
        }
        else if (type == BoxType.Persistent)
        {
            // Biến thành khối rỗng thay vì biến mất
            if (emptyBoxSprite != null) spriteRenderer.sprite = emptyBoxSprite;
            if (anim != null) anim.enabled = false; // Tắt anim để giữ sprite rỗng
        }
    }

    private void SpawnItems()
    {
        for (int i = 0; i < itemsPerHit; i++)
        {
            if (itemPrefabs.Length == 0) return;

            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            // Sinh ra item cao hơn hộp một chút
            GameObject item = Instantiate(prefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);

            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Tạo lực ngẫu nhiên trái/phải để item tản ra giống video
                float randomDir = Random.Range(-1f, 1f);
                Vector2 force = new Vector2(randomDir * launchForceX, launchForceY);
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}