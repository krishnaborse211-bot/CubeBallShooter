using UnityEngine;

// Single responsibility: detect climbable walls and handle climb movement.
public class PlayerClimb : MonoBehaviour
{
    [Header("Climbing Settings")]
    [SerializeField] private float climbSpeed         = 3f;
    [SerializeField] private float climbCheckDistance = 0.7f;

    private const float ClimbJumpPushback  = 0.5f;
    private const float ClimbCheckInterval = 0.05f;

    private CharacterController cc;

    // Timer only used for DETECTION (entering climb state)
    // NOT used when already climbing
    private float climbCheckTimer = 0f;

    public float ClimbCheckDistance => climbCheckDistance;

    public void Initialize(CharacterController characterController)
    {
        cc = characterController;
    }

    // ── THROTTLED: used in Movement + Jumping states ──
    // Only fires raycast 20 times/sec to detect if we SHOULD start climbing
    // Safe to throttle because we're not yet in climbing state
    public bool ShouldStartClimbing()
    {
        climbCheckTimer -= Time.deltaTime;
        if (climbCheckTimer > 0f) return false;
        climbCheckTimer = ClimbCheckInterval;

        return CheckWallInFront();
    }

    // ── UNTHROTTLED: used inside Climbing state ──
    // Checks every frame whether the wall is still there
    // Must be reliable — missing one frame drops the player
    public bool IsStillOnWall()
    {
        return CheckWallInFront();
    }

    // ── Shared raycast logic ──
    // Both methods above call this
    // Single place to change the raycast if needed later
    private bool CheckWallInFront()
    {
        if (!Physics.Raycast(transform.position, transform.forward,
                             out RaycastHit hit, climbCheckDistance))
            return false;

        return hit.collider.CompareTag("Climbable");
    }

    // Called every frame during Climbing state
    // In PlayerClimb.cs — change method signature:
    public void HandleClimbing(float verticalInput)
    {
    cc.Move(Vector3.up * verticalInput * climbSpeed * Time.deltaTime);
    }

    // Called when player jumps off wall
    public void PerformWallJump()
    {
        cc.Move(-transform.forward * ClimbJumpPushback);
    }
}