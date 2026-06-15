using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed  = 9f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float bodyRotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private BallShooter ballShooter; 

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -20f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;

    private const float GroundedVelocityReset = -2f;
   

    private CharacterController cc;
    private Vector3 velocity;
    private float coyoteTimeCounter;

    // ── Input references ──
    // These store the input VALUES, read once per frame
    // by PlayerController and passed down here
    private Vector2 moveInput;
    private bool    isSprinting;

    public Vector3 Velocity   => velocity;
    public bool    IsGrounded => cc.isGrounded;

    public void Initialize(CharacterController characterController)
    {
        cc = characterController;
    }

    // ── Called by PlayerController every frame ──
    // Passes current input values in, so PlayerMover
    // doesn't need to know about the Input System directly
    public void SetInput(Vector2 movement, bool sprinting)
    {
        moveInput   = movement;
        isSprinting = sprinting;
    }

  public void HandleGroundMovement()
{
    if (velocity.y < 0)
        velocity.y = GroundedVelocityReset;

    // TPP movement — relative to camera direction
    // not player body direction
    // W always = camera forward regardless of body facing
    Vector3 moveDir = mouseLook.GetCameraForward() * moveInput.y
                    + mouseLook.GetCameraRight()    * moveInput.x;

    // Slow down while aiming — competitive game feel
float aimMultiplier = (ballShooter != null && ballShooter.IsAiming)
    ? ballShooter.AimMoveMultiplier : 1f;
float currentSpeed = (isSprinting ? runSpeed : walkSpeed) * aimMultiplier;

    // Smooth body rotation toward movement direction
    // Player body faces WHERE they move, not where camera looks
    // This is exactly how PUBG/COD works
    if (moveDir.magnitude > 0.1f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * bodyRotationSpeed
        );
    }

    Vector3 finalMove = (moveDir * currentSpeed) + velocity;
    cc.Move(finalMove * Time.deltaTime);

    velocity.y += gravity * Time.deltaTime;
    coyoteTimeCounter = coyoteTime;
}

    public void HandleAirMovement()
{
    if (coyoteTimeCounter > 0)
        coyoteTimeCounter -= Time.deltaTime;

    // Same camera-relative movement in air
    Vector3 moveDir = mouseLook.GetCameraForward() * moveInput.y
                    + mouseLook.GetCameraRight()    * moveInput.x;

    Vector3 finalMove = (moveDir * walkSpeed) + velocity;
    cc.Move(finalMove * Time.deltaTime);

    velocity.y += gravity * Time.deltaTime;
}

    public bool TryJump()
    {
        if (coyoteTimeCounter <= 0) return false;

        velocity.y        = Mathf.Sqrt(jumpForce * -2f * gravity);
        coyoteTimeCounter = 0f;
        return true;
    }

    public void ResetVelocity()        { velocity.y = GroundedVelocityReset; }
    public void ZeroVerticalVelocity() { velocity.y = 0f; }

    public void ApplyWallJumpVelocity()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }
}