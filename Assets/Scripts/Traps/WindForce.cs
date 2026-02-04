using UnityEngine;

public class WindForce : MonoBehaviour
{
    public float windStrength = 12f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, windStrength);
            }
        }
    }
}
