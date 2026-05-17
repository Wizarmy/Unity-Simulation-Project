using UnityEngine;
using UnityEngine.InputSystem;

public class FreeFlightCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMoveMultiplier = 3f;
    [SerializeField] private float lookSensitivity = 2f;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isFastMove;

    private float _rotationX;
    private float _rotationY;

    private bool _cursorLocked = true;

    private void Awake()
    {
        _rotationX = transform.eulerAngles.x;
        _rotationY = transform.eulerAngles.y;
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        HandleCursorToggle();
        if (_cursorLocked)
        {
            HandleMovement();
            HandleRotation();
        }
    }

    private void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _cursorLocked = !_cursorLocked;
            if (_cursorLocked) LockCursor();
            else UnlockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed * (_isFastMove ? fastMoveMultiplier : 1f);

        Vector3 moveDir = Vector3.zero;

        moveDir += transform.forward * _moveInput.y;
        moveDir += transform.right * _moveInput.x;

        if (Keyboard.current.spaceKey.isPressed) moveDir += Vector3.up;
        if (Keyboard.current.ctrlKey.isPressed) moveDir -= Vector3.up;

        // Use unscaled time so camera moves normally even at 200x simulation speed
        transform.position += moveDir.normalized * (currentSpeed * Time.unscaledDeltaTime);
    }

    private void HandleRotation()
    {
        _rotationY += _lookInput.x * lookSensitivity;
        _rotationX -= _lookInput.y * lookSensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -89f, 89f);

        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    // ====================== INPUT SYSTEM CALLBACKS ======================
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnFastMove(InputAction.CallbackContext context)
    {
        _isFastMove = context.performed;
    }

    private void OnEnable()
    {
        LockCursor();
    }

    private void OnDisable()
    {
        UnlockCursor();
    }
}