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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isBroken)
        {
            isBroken = true;

            // chạy animation
            anim.SetTrigger("Break");

            // spawn mảnh sau 0.3s (đợi animation đầu chạy)
            Invoke("SpawnPieces", 0.3f);
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

        Destroy(gameObject, 0.5f);
    }
}