using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    private AudioManager audioManager;
    private void Awake(){
        gameManager=FindAnyObjectByType<GameManager>();
        audioManager=FindAnyObjectByType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.CompareTag("Items")){
            Destroy(collision.gameObject);
            audioManager.PlayCoinSound();
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
