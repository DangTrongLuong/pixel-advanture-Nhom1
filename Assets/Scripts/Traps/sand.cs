using UnityEngine;

public class sand : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Header("Cài đặt làm chậm")]
    [SerializeField] private float sandDrag = 10f; // Lực cản khi ở trong cát
    [SerializeField] private float normalDrag = 0f; // Lực cản bình thường

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra đúng nhân vật Player
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Đổi linearDrag thành drag
                rb.linearDamping = sandDrag; 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Đổi linearDrag thành drag
                rb.linearDamping = normalDrag;
            }
        }
    }
}
