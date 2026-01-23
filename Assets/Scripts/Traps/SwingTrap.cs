using UnityEngine;

public class SwingTrap : MonoBehaviour
{
    [Header("Swing Settings")]
    public float maxAngle = 60f;   // Góc vung (độ)
    public float speed = 2f;       // Tốc độ vung

    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        float angle = Mathf.Sin((Time.time - startTime) * speed) * maxAngle;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
