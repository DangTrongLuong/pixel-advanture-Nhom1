using UnityEngine;

public class MovingSaw : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    [SerializeField] private Vector2[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 200f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        MoveAlongPath();
        RotateSaw();
    }

    private void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[currentWaypointIndex],
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void RotateSaw()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }
}