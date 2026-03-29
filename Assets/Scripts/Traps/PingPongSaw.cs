using UnityEngine;

public class PingPongSaw : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    [SerializeField] private Vector2 moveOffset; 
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 300f;

    private Vector2 startPos;
    private Vector2 targetPos;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
    }

    void Update()
    {
        MovePingPong();
        RotateSaw();
    }

    private void MovePingPong()
    {
        float time = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector2.Lerp(startPos, targetPos, time);
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