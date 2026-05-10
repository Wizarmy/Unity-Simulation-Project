using UnityEngine;
using UnityEngine.InputSystem;

public class FreeFlightCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMoveMultiplier = 3f;
    [SerializeField] private float lookSensitivity = 2f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isFastMove;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private bool cursorLocked = true;

    private void Awake()
    {
        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        HandleCursorToggle();
        if (cursorLocked)
        {
            HandleMovement();
            HandleRotation();
        }
    }

    private void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            cursorLocked = !cursorLocked;

            if (cursorLocked)
                LockCursor();
            else
                UnlockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed * (isFastMove ? fastMoveMultiplier : 1f);

        Vector3 moveDir = Vector3.zero;

        moveDir += transform.forward * moveInput.y;
        moveDir += transform.right * moveInput.x;

        if (Keyboard.current.spaceKey.isPressed) moveDir += Vector3.up;
        if (Keyboard.current.ctrlKey.isPressed) moveDir -= Vector3.up;

        transform.position += moveDir.normalized * currentSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        rotationY += lookInput.x * lookSensitivity;
        rotationX -= lookInput.y * lookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -89f, 89f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    // ====================== INPUT SYSTEM CALLBACKS ======================
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnFastMove(InputAction.CallbackContext context)
    {
        isFastMove = context.performed;
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