using UnityEngine;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SETTINGS
    // ─────────────────────────────────────────
    [Header("Shooting Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform  shootPoint;
    [SerializeField] private float      shootForce   = 90f;
    [SerializeField] private float      ballLifetime = 2f;

    [Header("Aim Settings")]
    [SerializeField] private float normalFOV   = 60f;
    [SerializeField] private float aimFOV      = 45f;
    [SerializeField] private float fovSpeed    = 8f;
    // How much to slow player while aiming
    // PlayerMover reads this via IsAiming property
    [SerializeField] private float aimMoveMultiplier = 0.5f;

    [Header("References")]
    [SerializeField] private PlayerAnimator      playerAnimator;
    [SerializeField] private PlayerController    playerController;
    [SerializeField] private Camera              mainCamera;
    // lookTarget = the Look Transform that rotates with mouse
    // shooting toward this gives us up/down aim direction
    [SerializeField] private Transform           lookTarget;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   shootSound;

    // ─────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────
    private PlayerInputActions _inputActions;
    private bool               _isAiming;

    // PUBLIC — PlayerMover reads this to slow movement
    public bool  IsAiming          => _isAiming;
    public float AimMoveMultiplier => aimMoveMultiplier;

    // ─────────────────────────────────────────
    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()  => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    // ─────────────────────────────────────────
    private void Update()
    {
        HandleAim();
        HandleShoot();
    }

    // ─────────────────────────────────────────
    // AIM
    // ─────────────────────────────────────────
    private void HandleAim()
    {
        // Right mouse button hold = aim mode
        _isAiming = Mouse.current.rightButton.isPressed;

        // Smoothly lerp FOV between normal and aim
        // Lerp gives smooth zoom in/out feel
        float targetFOV = _isAiming ? aimFOV : normalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(
            mainCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSpeed
        );
    }

    // ─────────────────────────────────────────
    // SHOOT
    // ─────────────────────────────────────────
    private void HandleShoot()
    {
        // Migrated from old Input.GetMouseButtonDown
        // Now uses New Input System properly
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // Block shooting during traversal states
        if (playerController != null &&
            !playerController.CanShoot) return;

        Shoot();
    }

    // ─────────────────────────────────────────
    private void Shoot()
    {
        // Shoot toward lookTarget.forward
        // lookTarget rotates with mouse up/down
        // so player can now shoot at air targets
        // and upper hidden targets correctly
        Vector3    shootDirection = lookTarget.forward;
        GameObject ball           = Instantiate(
            ballPrefab,
            shootPoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Ball prefab has no Rigidbody!");
            return;
        }

        Physics.IgnoreCollision(
            ball.GetComponent<Collider>(),
            GetComponent<Collider>()
        );

        rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
        Destroy(ball, ballLifetime);

        if (playerAnimator != null)
            playerAnimator.PlayShootAnimation();

            if (audioSource != null && shootSound != null)
        audioSource.PlayOneShot(shootSound);
    }
}