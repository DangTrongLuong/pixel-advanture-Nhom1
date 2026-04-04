using UnityEngine;

public class StartCheckpoint : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng")]
    [SerializeField] private Animator flagAnimator; // Kéo vật thể Lá cờ vào đây
    [SerializeField] private string triggerName = "isPressed";
    [Header("Cấu hình Camera Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra đúng Tag Player của nhân vật
        if (!isActivated && collision.CompareTag("Player"))
        {
            ActivateFlag();
        }
    }

    private void ActivateFlag()
    {
        isActivated = true;
        
        if (flagAnimator != null)
        {
            // Kích hoạt animation của lá cờ
            if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);
        }
            flagAnimator.SetTrigger(triggerName);
            Debug.Log("Điểm xuất phát đã kích hoạt!");
        }
    }
}