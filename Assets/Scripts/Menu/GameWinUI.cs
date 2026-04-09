using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameWinUI : MonoBehaviour
{
    [Header("Hiển thị điểm (tùy chọn)")]
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private TextMeshProUGUI mapDisplay;

    private int _currentMapIndex = -1;
    private const int MAP_MAX = 17;
    private const string WIN_SCENE = "WinGame";

    private void Awake()
    {
        var buttons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveAllListeners();
            switch (btn.gameObject.name)
            {
                case "BtnPlayAgain": btn.onClick.AddListener(OnPlayAgain); break;
                case "BtnNextLevel": btn.onClick.AddListener(OnNextLevel); break;
                case "BtnLevel": btn.onClick.AddListener(OnLevel); break;
            }
        }
    }

    private void Start()
    {
        if (mapDisplay != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Map") && int.TryParse(sceneName.Substring(3), out int idx))
                mapDisplay.text = "Level " + idx;
            else
                mapDisplay.text = sceneName;
        }
    }

    public void Show(int score, int mapIndex)
    {
        _currentMapIndex = mapIndex;
        gameObject.SetActive(true);

        if (scoreDisplay != null) scoreDisplay.text = "Score: " + score;
        if (mapDisplay != null) mapDisplay.text = "Map " + mapIndex;
    }

    // ── BUTTONS ──────────────────────────────────────────

    public void OnPlayAgain()
    {
        if (PersistentPlayerManager.instance != null)
            PersistentPlayerManager.instance.ResetSelection();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextLevel()
    {
        Time.timeScale = 1f;

        if (_currentMapIndex == MAP_MAX)
        {
            SceneManager.LoadScene(WIN_SCENE);
            return;
        }

        if (_currentMapIndex > 0)
            SceneManager.LoadScene("Map" + (_currentMapIndex + 1));
    }

    public void OnLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level");
    }
}