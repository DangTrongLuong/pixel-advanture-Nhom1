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

    private void Awake()
    {
        // Tự tạo EventSystem nếu scene chưa có → fix lỗi button không click được
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogWarning("[MenuBtn] Không có EventSystem → Tự tạo mới.");
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Bind tất cả button trong children
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
        int current = GetCurrentMapIndex();
        if (current == -1) return;
        SceneManager.LoadScene("Map" + (current % MAP_MAX + 1));
    }

    public void OnPrev()
    {
        int current = GetCurrentMapIndex();
        if (current == -1) return;
        SceneManager.LoadScene("Map" + (current == MAP_MIN ? MAP_MAX : current - 1));
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