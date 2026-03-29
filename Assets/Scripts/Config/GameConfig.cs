using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sandSpeed = 2f;

    [Header("Jump")]
    public int maxJumps = 1;
    public float jumpForce = 15f;

    [Header("Wall")]
    public float wallSlideSpeed = -1.5f;
    public float wallJumpForce = 15f;
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpCooldown = 0.2f;
}