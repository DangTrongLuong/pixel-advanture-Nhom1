
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    void Start()
    {
        if (PersistentPlayerManager.instance == null)
        {
            Debug.LogWarning("PersistentPlayerManager chưa tồn tại → Tự tạo mới (vì đang chạy scene Map trực tiếp).");

            GameObject go = new GameObject("PersistentPlayerManager");
            go.AddComponent<PersistentPlayerManager>();
        }
        PersistentPlayerManager.instance.UpdatePlayersForNewScene();

        UpdateScore();
    }

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
