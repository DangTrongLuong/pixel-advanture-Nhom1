using UnityEngine;
using System.Collections;

public class AdvancedBreakableBox : MonoBehaviour
{
    public enum BoxType 
    { 
        BreakImmediately,      // Dạng 1: Chạm 1 lần -> Vỡ -> Rơi tất cả item
        EachHitDropsItem,      // Dạng 2: Chạm nhiều lần -> Vỡ (mỗi lần chạm rơi 1 item)
        AccumulateHitsAndDropAll // Dạng 3: Chạm đủ số lần -> Vỡ -> Rơi tất cả item
    }

    [Header("Cấu hình Loại Hộp")]
    [SerializeField] private BoxType type = BoxType.EachHitDropsItem;
    [SerializeField] private int maxHits = 3; 
    [SerializeField] private int itemsToSpawn = 3; 

    [Header("Cấu hình Camera Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    [Header("Cài đặt Vật phẩm")]
    [SerializeField] private GameObject[] itemPrefabs; 
    [SerializeField] private float launchForceX = 4f; 
    [SerializeField] private float launchForceY = 6f;

    [Header("Phản hồi nhân vật")]
    [SerializeField] private float playerBounceForce = 8f; 

    [Header("Animation")]
    [SerializeField] private float destroyDelay = 0.1f; 
    [SerializeField] private string hitTrigger = "doHit"; 

    private int currentHits;
    private bool isBroken = false;
    private Animator anim;
    private Collider2D col;

    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        currentHits = maxHits;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBroken || !collision.gameObject.CompareTag("Player")) return;

        // Lấy hướng của cú va chạm
        float contactY = collision.GetContact(0).normal.y;

        // contactY > 0.5f: Player đụng từ dưới lên (cụng đầu)
        // contactY < -0.5f: Player đụng từ trên xuống (nhảy lên đỉnh hộp)
        if (contactY > 0.5f || contactY < -0.5f)
        {
            HandleHit(collision);
        }
    }

    private void HandleHit(Collision2D collision)
    {
        currentHits--;

        // 1. Phản hồi hình ảnh của hộp
        if (anim != null) anim.SetTrigger(hitTrigger);

        // 2. Gọi rung màn hình thông qua Instance có sẵn
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);
        }

        // 3. Ép nhân vật nảy lên
        ApplyBounceToPlayer(collision.gameObject);

        // 4. Logic rơi item & vỡ theo từng dạng
        switch (type)
        {
            case BoxType.BreakImmediately:
                SpawnAllItems();
                BreakTheBox();
                break;

            case BoxType.EachHitDropsItem:
                SpawnSingleItem();
                if (currentHits <= 0) BreakTheBox();
                break;

            case BoxType.AccumulateHitsAndDropAll:
                if (currentHits <= 0)
                {
                    SpawnAllItems();
                    BreakTheBox();
                }
                break;
        }
    }

    private void ApplyBounceToPlayer(GameObject player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Animator playerAnim = player.GetComponent<Animator>();

        if (playerRb != null)
        {
            // Sử dụng linearVelocity cho bản Unity của bạn
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
            playerRb.AddForce(Vector2.up * playerBounceForce, ForceMode2D.Impulse);
        }

        // Chạy animation nhảy của nhân vật
        if (playerAnim != null)
        {
            playerAnim.Play("PlayerJump"); 
        }
    }

    public void BreakTheBox()
    {
        isBroken = true;
        col.enabled = false; 
        Destroy(gameObject, destroyDelay);
    }

    private void SpawnSingleItem()
    {
        if (itemPrefabs.Length == 0) return;
        
        GameObject selectedPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        Vector2 spawnPos = (Vector2)transform.position + Vector2.up * 0.6f;
        GameObject item = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        if (itemRb != null)
        {
            float randomX = Random.Range(-1f, 1f) * launchForceX;
            itemRb.AddForce(new Vector2(randomX, launchForceY), ForceMode2D.Impulse);
        }
    }

    private void SpawnAllItems()
    {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            SpawnSingleItem();
        }
    }
}