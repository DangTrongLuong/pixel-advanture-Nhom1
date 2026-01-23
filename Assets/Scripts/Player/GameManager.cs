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

    public GameObject RandomPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy player nào với tag 'Player'");
            return null;
        }

        int randomIndex = Random.Range(0, allPlayers.Length);
        GameObject selectedPlayer = allPlayers[randomIndex];

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (i == randomIndex)
                allPlayers[i].SetActive(true);
            else
                allPlayers[i].SetActive(false);
        }

        return selectedPlayer;
    }
}
