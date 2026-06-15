using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; }

    // ── Component references ──
    private CharacterController cc;
    private PlayerMover         mover;
    private PlayerVault         vault;
    private PlayerClimb         climb;

    // ── Input System ──
    private PlayerInputActions inputActions;

    // ── Input values ──
    private Vector2 moveInput;
    private bool    sprintInput;
    private bool    jumpPressedThisFrame;

    // ── Buffers ──
    private float _groundedBuffer;
    private float _jumpStateBuffer;
    private const float GroundedBufferTime   = 0.15f;
    private const float MinJumpStateDuration = 0.2f;
    

    // ── Jump was intentional (spacebar) ──
    // Tracks if current jump was from spacebar
    // or from walking off edge
    private bool _intentionalJump;

    // ─────────────────────────────────────────
    private void Awake()
    {
        cc    = GetComponent<CharacterController>();
        mover = GetComponent<PlayerMover>();
        vault = GetComponent<PlayerVault>();
        climb = GetComponent<PlayerClimb>();

        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        mover.Initialize(cc);
        vault.Initialize(cc);
        climb.Initialize(cc);

        CurrentState      = PlayerState.Movement;
        _groundedBuffer   = GroundedBufferTime;
        _intentionalJump  = false;
    }

    private void Update()
    {
        ReadInput();
        mover.SetInput(moveInput, sprintInput);

        switch (CurrentState)
        {
            case PlayerState.Movement: HandleMovementState(); break;
            case PlayerState.Jumping:  HandleJumpingState();  break;
            case PlayerState.Climbing: HandleClimbingState(); break;
            case PlayerState.Vaulting: HandleVaultingState(); break;
        }
    }

    private void ReadInput()
    {
        moveInput            = inputActions.Player.Move.ReadValue<Vector2>();
        sprintInput          = inputActions.Player.Sprint.IsPressed();
        jumpPressedThisFrame = inputActions.Player.Jump.WasPressedThisFrame();
    }

    // ─────────────────────────────────────────
    private void HandleMovementState()
    {
        if (climb.ShouldStartClimbing())
        {
            _intentionalJump = false;
            CurrentState     = PlayerState.Climbing;
            return;
        }

        if (vault.CanVault(moveInput.y))
        {
            _intentionalJump = false;
            CurrentState     = PlayerState.Vaulting;
            return;
        }

        // Spacebar pressed = intentional jump
        // _intentionalJump = true tells animator
        // to show jump animation
        if (jumpPressedThisFrame && mover.TryJump())
        {
            _intentionalJump = true;
            _groundedBuffer  = 0f;
            _jumpStateBuffer = MinJumpStateDuration;
            CurrentState     = PlayerState.Jumping;
            return;
        }

        mover.HandleGroundMovement();

        // Walked off edge = NOT intentional jump
        // Physics still needs Jumping state for gravity
        // But animator will NOT show jump animation
        if (mover.IsGrounded)
        {
            _groundedBuffer = GroundedBufferTime;
        }
        else
        {
            _groundedBuffer -= Time.deltaTime;
        }

        if (_groundedBuffer <= 0f)
        {
            _intentionalJump = false; // edge fall = no jump anim
            _jumpStateBuffer = 0f;
            CurrentState     = PlayerState.Jumping;
        }
    }

    // ─────────────────────────────────────────
    private void HandleJumpingState()
    {
        _jumpStateBuffer -= Time.deltaTime;

        // Mid-air climb detection
        // Highest priority — checked every frame
        if (climb.ShouldStartClimbing())
        {
            _intentionalJump = false;
            CurrentState     = PlayerState.Climbing;
            return;
        }

        mover.HandleAirMovement();

        if (mover.IsGrounded && _jumpStateBuffer <= 0f)
        {
            mover.ResetVelocity();
            _intentionalJump = false;
            _groundedBuffer  = GroundedBufferTime;
            CurrentState     = PlayerState.Movement;
        }
    }

    // ─────────────────────────────────────────
    private void HandleClimbingState()
    {
        if (!climb.IsStillOnWall())
        {
            _groundedBuffer = GroundedBufferTime;
            CurrentState    = PlayerState.Movement;
            return;
        }

        if (jumpPressedThisFrame)
        {
            climb.PerformWallJump();
            mover.ApplyWallJumpVelocity();
            _intentionalJump = true;
            CurrentState     = PlayerState.Jumping;
            return;
        }

        climb.HandleClimbing(moveInput.y);
        mover.ZeroVerticalVelocity();
    }

    // ─────────────────────────────────────────
    private void HandleVaultingState()
    {
        bool vaultComplete = vault.HandleVault();

        if (vaultComplete)
        {
            _groundedBuffer  = GroundedBufferTime;
            _intentionalJump = false;
            mover.ResetVelocity();
            CurrentState = PlayerState.Movement;
        }
    } 

    public bool CanShoot =>
    CurrentState != PlayerState.Vaulting &&
    CurrentState != PlayerState.Climbing &&
    CurrentState != PlayerState.Jumping;
    
    // ─────────────────────────────────────────
    // PUBLIC — PlayerAnimator reads this
    // to decide if jump animation should play
    public float VerticalInput => moveInput.y;
    public bool IsIntentionalJump => _intentionalJump;
}