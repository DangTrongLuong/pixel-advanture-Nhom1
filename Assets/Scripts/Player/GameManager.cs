using UnityEngine;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateScore();
        PersistentPlayerManager.instance.UpdatePlayersForNewScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddScore(int points){
        score += points;
        UpdateScore();
    }
    private void UpdateScore(){
        scoreText.text = score.ToString();
    }

}
