using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SETTINGS
    // ─────────────────────────────────────────
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalClampMin = -40f;
    [SerializeField] private float verticalClampMax = 70f;

    [Header("References")]
    // Look = empty child of Player at Y:1.5
    // Camera orbits around this point
    [SerializeField] private Transform lookTarget;

    // ─────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────
    private PlayerInputActions inputActions;
    private float _yaw;   // horizontal rotation
    private float _pitch; // vertical rotation

    // PUBLIC — PlayerMover reads this to move
    // in camera forward direction
    public Transform LookTarget => lookTarget;

    // ─────────────────────────────────────────
    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Initialize yaw to current player facing
        // so camera doesn't snap on game start
        _yaw   = transform.eulerAngles.y;
        _pitch = 0f;
    }

    private void OnEnable()  => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    // ─────────────────────────────────────────
    private void Update()
    {
        HandleCameraRotation();
    }

    // ─────────────────────────────────────────
    private void HandleCameraRotation()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Accumulate rotation values
        // mouseSensitivity controls how fast camera moves
        _yaw   += lookInput.x * mouseSensitivity;
        _pitch -= lookInput.y * mouseSensitivity;

        // Clamp vertical so camera doesn't flip
        _pitch = Mathf.Clamp(_pitch, verticalClampMin, verticalClampMax);

        // Rotate Look target — camera orbits around this
        // Yaw  = horizontal (left/right)
        // Pitch = vertical  (up/down)
        lookTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // ─────────────────────────────────────────
    // PUBLIC — PlayerMover calls this to get
    // camera's flat forward direction for movement
    // Flat = no Y component so player doesn't
    // move up/down when camera looks up/down
    public Vector3 GetCameraForward()
    {
        Vector3 forward = lookTarget.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = lookTarget.right;
        right.y = 0f;
        return right.normalized;
    }
}