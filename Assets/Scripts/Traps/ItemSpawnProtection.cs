using UnityEngine;
using System.Collections;

public class ItemSpawnProtection : MonoBehaviour
{
    public bool canBePickedUp = false; // Biến để kiểm tra trạng thái nhặt
    [SerializeField] private float protectionTime = 0.5f; // Thời gian chờ để bay ra

    IEnumerator Start()
    {
        // Khi vừa sinh ra, chưa cho phép nhặt ngay
        canBePickedUp = false;
        
        // Chờ một khoảng thời gian ngắn để lực vật lý đẩy item đi
        yield return new WaitForSeconds(protectionTime);
        
        // Sau đó mới cho phép nhặt
        canBePickedUp = true;
    }
}