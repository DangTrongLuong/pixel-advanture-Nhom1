using UnityEngine;

public class SwingSpiked : MonoBehaviour
{
    public enum RotationMode { Pendulum180, FullRotation360, Static }

    [Header("Cấu hình Chế độ")]
    [SerializeField] private RotationMode mode = RotationMode.Pendulum180;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxAngle = 90f;

    void Update()
    {
        if (mode == RotationMode.Static) return;

        float angle = 0f;

        if (mode == RotationMode.Pendulum180)
        {
            angle = Mathf.Sin(Time.time * speed) * maxAngle;
        }
        else if (mode == RotationMode.FullRotation360)
        {
            angle = Time.time * speed * -100f;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }
}