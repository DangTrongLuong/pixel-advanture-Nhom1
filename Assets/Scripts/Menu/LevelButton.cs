using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("Scene")]
    public string sceneName;

    [Header("Lock UI")]
    public GameObject lockIcon; 
    public Button button;   

    private int _mapIndex = -1;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (!string.IsNullOrEmpty(sceneName) &&
            sceneName.StartsWith("Map") &&
            int.TryParse(sceneName.Substring(3), out int idx))
        {
            _mapIndex = idx;
        }
    }

    void Start()
    {
        ApplyLockState();
    }

    public void ApplyLockState()
    {
        if (_mapIndex == -1) return;

        GameProgress progress = SaveSystem.LoadProgress();
        bool unlocked = _mapIndex <= progress.highestUnlockedMap;
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (button != null)
            button.interactable = unlocked;
    }

    public void LoadThisLevel()
    {
        if (_mapIndex == -1) return;

        GameProgress progress = SaveSystem.LoadProgress();
        if (_mapIndex > progress.highestUnlockedMap) return;

        if (PersistentPlayerManager.instance == null)
        {
            GameObject go = new GameObject("PersistentPlayerManager");
            go.AddComponent<PersistentPlayerManager>();
        }

        PersistentPlayerManager.instance.SelectRandomPlayer();
        SceneManager.LoadScene(sceneName);
    }
}