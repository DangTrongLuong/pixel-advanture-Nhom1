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
        // Nếu instance chưa tồn tại → tự tạo ngay lập tức
        if (PersistentPlayerManager.instance == null)
        {
            Debug.LogWarning("PersistentPlayerManager chưa tồn tại → Tự tạo mới (vì đang chạy scene Map trực tiếp).");

            GameObject go = new GameObject("PersistentPlayerManager");
            go.AddComponent<PersistentPlayerManager>();
        }

        // Tới đây chắc chắn instance không còn null
        PersistentPlayerManager.instance.UpdatePlayersForNewScene();

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

}
