using UnityEngine;

public class FanController : MonoBehaviour
{
    [SerializeField] private bool isOn = true;
    [SerializeField] private float windStrength = 20f;

    private AreaEffector2D effector;
    private Animator anim;

    void Start()
    {
        effector = GetComponent<AreaEffector2D>();
        anim = GetComponent<Animator>();
        UpdateFanState();
    }

    // Hàm để bật/tắt quạt bằng code (ví dụ nhân vật gạt cần gạt)
    public void ToggleFan()
    {
        isOn = !isOn;
        UpdateFanState();
    }

    private void UpdateFanState()
    {
        if (effector != null)
            effector.forceMagnitude = isOn ? windStrength : 0;

        if (anim != null)
            anim.SetBool("isSpinning", isOn);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Kiểm tra đúng nhân vật thông qua Tag "Player"
        if (isOn && collision.CompareTag("Player"))
        {
            // Có thể thêm hiệu ứng bụi hoặc âm thanh tại đây
            Debug.Log("Nhân vật đang bay trong gió!");
        }
    }
}