using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuBtn : MonoBehaviour
{
    private const int MAP_MIN = 1;
    private const int MAP_MAX = 17;
    private static bool isMuted = false;

    [Header("Volume Icon (tuỳ chọn)")]
    [SerializeField] private Image volumeIcon;
    [SerializeField] private Sprite iconVolumeOn;
    [SerializeField] private Sprite iconVolumeOff;

    private const string WIN_SCENE = "WinGame";

    private void Awake()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning("[MenuBtn] Không có EventSystem → Tự tạo mới.");
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        Button[] allButtons = GetComponentsInChildren<Button>(includeInactive: true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.RemoveAllListeners();
            switch (btn.gameObject.name)
            {
                case "BtnRestart": btn.onClick.AddListener(OnRestart); break;
                case "BtnNext": btn.onClick.AddListener(OnNext); break;
                case "BtnPrev": btn.onClick.AddListener(OnPrev); break;
                case "BtnLevel": btn.onClick.AddListener(OnLevel); break;
                case "BtnVolume": btn.onClick.AddListener(OnVolume); break;
            }
        }
    }

    private void Start()
    {
        AudioListener.volume = isMuted ? 0f : 1f;
        UpdateVolumeIcon();
    }

    public void OnRestart()
    {
        if (PersistentPlayerManager.instance != null)
            PersistentPlayerManager.instance.ResetSelection();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNext()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Map17")
        {
            SaveSystem.UnlockMap(MAP_MAX);
            SceneManager.LoadScene(WIN_SCENE);
            return;
        }

        if (currentScene == WIN_SCENE)
        {
            SceneManager.LoadScene("Map1");
            return;
        }

        int current = GetCurrentMapIndex();
        if (current == -1) return;

        SaveSystem.UnlockMap(current + 1);

        SceneManager.LoadScene("Map" + (current + 1));
    }

    public void OnPrev()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Map1")
        {
            SceneManager.LoadScene(WIN_SCENE);
            return;
        }

        if (currentScene == WIN_SCENE)
        {
            SceneManager.LoadScene("Map17");
            return;
        }

        int current = GetCurrentMapIndex();
        if (current == -1) return;

        SceneManager.LoadScene("Map" + (current - 1));
    }

    public void OnLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void OnVolume()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        UpdateVolumeIcon();
    }

    private int GetCurrentMapIndex()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "WinGame")
            return MAP_MAX;
        if (sceneName.StartsWith("Map") && int.TryParse(sceneName.Substring(3), out int index))
            return index;
        Debug.LogWarning("[MenuBtn] Scene không phải dạng MapX: " + sceneName);
        return -1;
    }

    private void UpdateVolumeIcon()
    {
        if (volumeIcon == null) return;
        volumeIcon.sprite = isMuted ? iconVolumeOff : iconVolumeOn;
    }
}