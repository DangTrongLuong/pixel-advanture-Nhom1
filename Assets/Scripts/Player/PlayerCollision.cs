using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    private void Awake(){
        gameManager=FindAnyObjectByType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.CompareTag("Items")){
            Destroy(collision.gameObject);
            gameManager.AddScore(1);
        }

        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
