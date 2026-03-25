using UnityEngine;

public class BreakWood : MonoBehaviour
{
    public GameObject pieceLeft;
    public GameObject pieceRight;

    private Animator anim;
    private bool isBroken = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isBroken)
        {
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                        rb.AddForce(Vector2.up * 8f, ForceMode2D.Impulse);
                    }

                    BreakBox();
                    return;
                }

                if (contact.normal.y > 0.5f)
                {
                    BreakBox();
                    return;
                }
            }
        }
    }

    void SpawnPieces()
    {
        GameObject left = Instantiate(pieceLeft, transform.position + new Vector3(-0.2f, 0, 0), Quaternion.identity);
        Rigidbody2D rbLeft = left.GetComponent<Rigidbody2D>();
        rbLeft.AddForce(new Vector2(-2f, 4f), ForceMode2D.Impulse);

        GameObject right = Instantiate(pieceRight, transform.position + new Vector3(0.2f, 0, 0), Quaternion.identity);
        Rigidbody2D rbRight = right.GetComponent<Rigidbody2D>();
        rbRight.AddForce(new Vector2(2f, 4f), ForceMode2D.Impulse);

        Destroy(left, 1.5f);
        Destroy(right, 1.5f);

        Destroy(gameObject);
    }

    void BreakBox()
    {
        isBroken = true;

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.1f, 0.15f);

        anim.SetTrigger("Break");
        Invoke(nameof(SpawnPieces), 0.3f);
    }
}