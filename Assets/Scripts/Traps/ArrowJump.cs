using UnityEngine;

public class ArrowJump : MonoBehaviour
{
    public float jumpForce = 22f;

    Animator anim;
    bool used = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        used = true;

        anim.SetTrigger("hit");

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        Destroy(gameObject, 0.3f);
    }
}
