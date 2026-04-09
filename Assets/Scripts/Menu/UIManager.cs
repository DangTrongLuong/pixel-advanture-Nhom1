using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public Button btnPlayAgain;
    public Button btnLevel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameOverUI.SetActive(false);

        btnPlayAgain.onClick.AddListener(OnPlayAgain);
        btnLevel.onClick.AddListener(OnLevel);
    }

    public void ShowGameOver()
    {
        gameOverUI.SetActive(true);
    }

    void OnPlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnLevel()
    {
        SceneManager.LoadScene("Level");
    }
}
