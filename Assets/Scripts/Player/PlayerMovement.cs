using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input Reference")]
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxJumpTime = 0.3f;
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;

    // Use singleton instance for Gravity Switcher
    private GravityDirection CurrentGravity => GravitySwitcher.Instance.CurrentGravityDirection;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;

    // --- Internal Cached Variables ---
    private Vector2 _currentMoveInput;
    private Vector2 _currentLookInput;
    private bool _isSprinting;
    private bool _isJumpHeld;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Start()
    {
        rb.freezeRotation = true;
        rb.useGravity = true;
        Cursor.lockState = CursorLockMode.Locked;

        AlignPlayerToGravity();
    }

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            // Subscribe to value change events
            inputHandler.OnMove += UpdateMoveInput;
            inputHandler.OnLook += UpdateLookInput;
            inputHandler.OnSprint += UpdateSprintState;
            inputHandler.OnJump += UpdateJumpState;

            // Subscribe to trigger events
            inputHandler.OnJumpTriggered += HandleJumpTriggered;
            // inputHandler.OnPauseTriggered += HandleUnlockCursor; // Handled in InputHandler SetInputMap now
            inputHandler.OnGravityChange += ChangeGravityState;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            // Unsubscribe
            inputHandler.OnMove -= UpdateMoveInput;
            inputHandler.OnLook -= UpdateLookInput;
            inputHandler.OnSprint -= UpdateSprintState;
            inputHandler.OnJump -= UpdateJumpState;

            inputHandler.OnJumpTriggered -= HandleJumpTriggered;
            // inputHandler.OnPauseTriggered -= HandleUnlockCursor;
            inputHandler.OnGravityChange -= ChangeGravityState;
        }
    }

    void Update()
    {
        CheckGround();
        HandleMouseLook();
        HandleMovement();
    }

    // --- Event Handlers (Internal State Updates) ---

    private void UpdateMoveInput(Vector2 input) => _currentMoveInput = input;
    private void UpdateLookInput(Vector2 input) => _currentLookInput = input;
    private void UpdateSprintState(bool isSprinting) => _isSprinting = isSprinting;
    private void UpdateJumpState(bool isHeld)
    {
        _isJumpHeld = isHeld;
        if (!isHeld) isJumping = false; // Stop jump ascent if button released
    }

    private void HandleJumpTriggered()
    {
        if (isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
        }
    }

    private void ChangeGravityState(int directionDelta)
    {
        if (GravitySwitcher.Instance == null) return;

        int current = (int)CurrentGravity;
        int next = (current + directionDelta) % 4;
        if (next < 0) next += 4;

        GravitySwitcher.Instance.SwitchGravity((GravityDirection)next);
        AlignPlayerToGravity();
    }

    // --- Main Logic ---

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }

    public void AlignPlayerToGravity()
    {
        if (GravitySwitcher.Instance == null) return;
        
        float targetZ = 0f;
        switch (CurrentGravity)
        {
            case GravityDirection.SOUTH: targetZ = 0f; break;
            case GravityDirection.WEST: targetZ = -90f; break;
            case GravityDirection.NORTH: targetZ = 180f; break;
            case GravityDirection.EAST: targetZ = 90f; break;
        }
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, targetZ);
    }

    void HandleMouseLook()
    {
        float mouseX = _currentLookInput.x * mouseSensitivity;
        float mouseY = _currentLookInput.y * mouseSensitivity;

        // 1. Horizontal Rotation (Player Body)
        transform.Rotate(Vector3.up * mouseX);

        // 2. Vertical Rotation (Camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        Vector3 localMoveDir = new Vector3(_currentMoveInput.x, 0f, _currentMoveInput.y).normalized;

        float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;

        // 2. Convert current world velocity to local velocity
        // localVelocity.y is always "vertical speed relative to character (gravity)"
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float currentVerticalSpeed = localVelocity.y;

        // 3. Jump Logic (Control Local Y)
        if (isJumping)
        {
            if (_isJumpHeld && jumpTimeCounter > 0)
            {
                currentVerticalSpeed = jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // 4. Combine Final Local Velocity
        // Overwrite X, Z with input (apply speed), keep Y (gravity/jump)
        Vector3 finalLocalVelocity = new Vector3(
            localMoveDir.x * currentSpeed,
            currentVerticalSpeed,
            localMoveDir.z * currentSpeed
        );

        // 5. Convert Local Velocity back to World Velocity and Apply
        // TransformDirection applies character rotation (gravity direction) to world coordinates
        rb.linearVelocity = transform.TransformDirection(finalLocalVelocity);
    }
}