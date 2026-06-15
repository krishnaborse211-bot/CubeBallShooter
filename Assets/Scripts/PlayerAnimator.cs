using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // ─────────────────────────────────────────
    // HASHED PARAMETERS
    // ─────────────────────────────────────────
    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash  = Animator.StringToHash("IsRunning");
    private static readonly int IsClimbingHash = Animator.StringToHash("IsClimbing");
    private static readonly int IsVaultingHash = Animator.StringToHash("IsVaulting");
    private static readonly int IsFallingHash  = Animator.StringToHash("IsFalling");
    private static readonly int JumpHash       = Animator.StringToHash("Jump");
    private static readonly int ShootHash      = Animator.StringToHash("Shoot");
    private static readonly int ClimbSpeedHash = Animator.StringToHash("ClimbSpeed");
    
    
    // ─────────────────────────────────────────
    // REFERENCES
    // ─────────────────────────────────────────
    [SerializeField] private Animator            animator;
    [SerializeField] private PlayerController    playerController;
    [SerializeField] private CharacterController characterController;

    // ─────────────────────────────────────────
    // TUNING
    // ─────────────────────────────────────────
    [SerializeField] private float runSpeed              = 9f;
    [SerializeField] private float groundCheckDistance   = 0.3f;
    [SerializeField] private float fallVelocityThreshold = -3f;
    [SerializeField] private float airTimeForFall        = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    // ─────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────
    private PlayerState _previousState;
    private float       _currentSpeed;
    private float       _airTimer;
    private bool _isFalling;
    private bool _wasRunning; // ← ADD THIS
    private const float SpeedDampTime = 0.1f;
    

    // ─────────────────────────────────────────
    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        _previousState = PlayerState.Movement;
        _airTimer      = 0f;
        _isFalling     = false;
    }

    private void Update()
    {
        PlayerState currentState = playerController.CurrentState;

        UpdateLocomotion(currentState);
        UpdateGrounded();
        UpdateFalling(currentState);
        UpdateStateParameters(currentState);
        HandleStateTransitions(currentState);

        _previousState = currentState;
    }

    // ─────────────────────────────────────────
    // LOCOMOTION
    // ─────────────────────────────────────────
    private void UpdateLocomotion(PlayerState currentState)
    {
        // Special states block locomotion completely
      
        if (currentState == PlayerState.Climbing ||
            currentState == PlayerState.Vaulting  ||
            currentState == PlayerState.Jumping)
        {
            _currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        // NOTE: do NOT set IsRunning false here anymore
        // _wasRunning preserves the value for RunningJump check
        return;
        }
        _wasRunning = animator.GetBool(IsRunningHash);

        Vector3 horizontalVelocity = new Vector3(
            characterController.velocity.x,
            0f,
            characterController.velocity.z
        );

        // Divide by runSpeed to normalize 0→1
        // runSpeed must match PlayerMover run speed exactly
        float targetSpeed = Mathf.Clamp01(
    horizontalVelocity.magnitude / (runSpeed * 0.95f)

        );

        // Smooth damp prevents snapping
        _currentSpeed = Mathf.Lerp(
            _currentSpeed,
            targetSpeed,
            SpeedDampTime
        );

        
        animator.SetFloat(SpeedHash, _currentSpeed);
Debug.Log("Speed: " + _currentSpeed + " | IsRunning: " + (targetSpeed > 0.6f) + " | RawVelocity: " + horizontalVelocity.magnitude);

        // IsRunning = true drives RunningJump condition
        // Threshold 0.6 = above walk speed
        animator.SetBool(IsRunningHash, targetSpeed > 0.6f);
    }

    // ─────────────────────────────────────────
    // GROUNDED
    // ─────────────────────────────────────────
    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            characterController.height / 2f + groundCheckDistance,
            groundLayer
        );
    }

    private void UpdateGrounded()
    {
        animator.SetBool(IsGroundedHash, IsGrounded());
    }

    // ─────────────────────────────────────────
    // FALLING
    // ─────────────────────────────────────────
    private void UpdateFalling(PlayerState currentState)
{
    // ONLY these two block falling
    // Jumping is ALLOWED to transition to falling
    if (currentState == PlayerState.Climbing ||
        currentState == PlayerState.Vaulting)
    {
        _airTimer  = 0f;
        _isFalling = false;
        animator.SetBool(IsFallingHash, false);
        return;
    }

    bool grounded      = IsGrounded();
    bool goingDownFast = characterController.velocity.y
                         < fallVelocityThreshold;

    if (!grounded && goingDownFast)
    {
        _airTimer += Time.deltaTime;
    }
    else
    {
        _airTimer  = 0f;
        _isFalling = false;
    }

    // Your rule: 0.5s falling fast = FallingIdle
    if (_airTimer >= airTimeForFall)
        _isFalling = true;

    animator.SetBool(IsFallingHash, _isFalling);
}

    // ─────────────────────────────────────────
    // STATE PARAMETERS
    // ─────────────────────────────────────────
   private void UpdateStateParameters(PlayerState currentState)
{
    animator.SetBool(IsClimbingHash,
        currentState == PlayerState.Climbing
        
    );

    animator.SetBool( IsVaultingHash,
        currentState == PlayerState.Vaulting
       
    );

    if (currentState == PlayerState.Climbing)
    {
        animator.SetFloat(
            ClimbSpeedHash,
            playerController.VerticalInput
        );
    }
}

    // ─────────────────────────────────────────
    // STATE TRANSITIONS
    // ─────────────────────────────────────────
    private void HandleStateTransitions(PlayerState currentState)
    {
        if (currentState == _previousState) return;

        // Clear ALL triggers on every state change
        // Prevents queued triggers firing at wrong time
        animator.ResetTrigger(JumpHash);
        animator.ResetTrigger(ShootHash);

        // Jump animation rule:
        // ONLY fires when spacebar pressed from ground
        // IsIntentionalJump = false means edge fall
        // Edge fall = no jump animation, FallingIdle handles it
       if (currentState  == PlayerState.Jumping  &&
         _previousState == PlayerState.Movement &&
        playerController.IsIntentionalJump)
    {
        animator.SetBool(IsRunningHash, _wasRunning);
        Debug.Log("JUMP FIRED | WasRunning: " + _wasRunning);
        animator.SetTrigger(JumpHash);
    }

        // Climbing cancels jump animation immediately
        if (currentState == PlayerState.Climbing)
        {
            animator.ResetTrigger(JumpHash);
        }
    }

    // ─────────────────────────────────────────
    // PUBLIC
    // ─────────────────────────────────────────
    public void PlayShootAnimation()
    {
        animator.SetTrigger(ShootHash);
    }
}