using UnityEngine;

public class SoftBlock : MonoBehaviour
{
    public Sprite normalSprite;   // hộp bình thường
    public Sprite crackSprite;    // hộp bị nứt
    public GameObject breakEffect; // hiệu ứng vỡ (optional)

    private SpriteRenderer sr;
    private int hitCount = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = normalSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Hit();
        }
    }

    void Hit()
    {
        hitCount++;

        if (hitCount == 1)
        {
            // Lần 1 → nứt
            sr.sprite = crackSprite;
        }
        else if (hitCount >= 2)
        {
            // Lần 2 → vỡ
            Break();
        }
    }

    void Break()
    {
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
