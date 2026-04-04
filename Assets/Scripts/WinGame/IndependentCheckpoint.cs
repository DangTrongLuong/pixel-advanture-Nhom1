using UnityEngine;

public class IndependentCheckpoint : MonoBehaviour
{
    public enum CheckpointType { Intermediate, Finish }

    [Header("Cấu hình Loại")]
    [SerializeField] private CheckpointType type = CheckpointType.Intermediate;

    [Header("Kết nối Animator")]
    // Kéo ĐÚNG lá cờ hoặc cái cúp tương ứng với bục này vào đây
    [SerializeField] private Animator visualAnimator; 
    [SerializeField] private string triggerName = "Activate";
    [Header("Cấu hình Camera Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [Header("Hệ thống Pháo hoa Đa dạng")]
    // Đổi từ 1 ParticleSystem thành một mảng (Array) để chứa nhiều loại pháo
    [SerializeField] private ParticleSystem[] fireworkParticles;

    private bool isReached = false; // Biến này là 'private' (không static) để mỗi bục tự quản lý trạng thái của nó

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra đúng nhân vật thông qua Tag
        if (!isReached && collision.CompareTag("Player"))
        {
            Reached();
        }
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);
        }
        foreach (ParticleSystem ps in fireworkParticles)
            {
                if (ps != null) ps.Play(); 
                Debug.Log("Pháo hoa");

            }
    }

    private void Reached()
    {
        isReached = true;

        // 1. Kích hoạt Animation cho riêng vật thể được kéo vào ô visualAnimator
        if (visualAnimator != null)
        {
            visualAnimator.SetTrigger(triggerName);
            
        }

        // 2. Nếu là điểm về đích (Finish), có thể gọi lệnh thắng cuộc từ GameManager
        if (type == CheckpointType.Finish)
        {
            
            Debug.Log("Chúc mừng! Bạn đã về đích!");
            
        }
        else
        {
            // 3. Nếu là checkpoint giữa đường, lưu vị trí hồi sinh vào một nơi trung gian
            // (Chúng ta sẽ cập nhật GameManager để giữ vị trí này)
            Debug.Log($"Đã lưu Checkpoint tại: {transform.position}");
        }
    }
}