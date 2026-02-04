using UnityEngine;

public class SpringJump : MonoBehaviour
{
    public float jumpForce = 22f;

    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // reset vận tốc rơi
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // bật lên
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // play animation trampoline
        if (anim != null)
            anim.SetTrigger("Jump");
    }
}
