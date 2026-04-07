using UnityEngine;

public class TerrainZone : MonoBehaviour
{
    public enum TerrainType { Sand, Swamp, Ice }
    [Header("Cấu hình Loại Địa hình")]
    public TerrainType type;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var controller = collision.GetComponent<PlayerController>();
            if (controller != null) controller.SetTerrain(type, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var controller = collision.GetComponent<PlayerController>();
            if (controller != null) controller.SetTerrain(type, false);
        }
    }
}