using UnityEngine;

public class Break : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public GameObject explosionPrefab;

public void OnTriggerEnter2D(Collider2D collision) => Die();

protected virtual void Die()
{
var explosion = Instantiate(explosionPrefab, transform.position,
transform.rotation);
Destroy(explosion, 1);
Destroy(gameObject);
}
}
