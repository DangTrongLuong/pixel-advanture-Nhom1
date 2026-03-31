using UnityEngine;
using System.Collections;

public class ItemSpawnProtection : MonoBehaviour
{
    public bool canBePickedUp = false; 
    [SerializeField] private float protectionTime = 0.5f; // Thời gian chờ (giây)

    IEnumerator Start()
    {
        canBePickedUp = false;
        // Chờ vật phẩm bay ra khỏi vị trí của hộp/nhân vật
        yield return new WaitForSeconds(protectionTime);
        canBePickedUp = true;
    }
}