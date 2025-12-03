using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxJumpTime = 0.3f;
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("마우스 시점 설정")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("카메라")]
    [SerializeField] private Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float zRotation = 0f;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;

    // viewState: 0 = Default, 1 = Left, 2 = UpDown, 3 = Right
    private enum ViewState { Default = 0, Left = 1, UpDown = 2, Right = 3 }
    private ViewState viewState = ViewState.Default;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

        xRotation = transform.eulerAngles.x;
        yRotation = transform.eulerAngles.y;
        zRotation = transform.eulerAngles.z;
        
        int state = Mathf.RoundToInt(zRotation / 90f) % 4;
        if (state < 0) state += 4;
        viewState = (ViewState)state;
        zRotation = state * 90f;
    }

    void Update()
    {
        CheckGround();
        HandleViewRotation();
        HandleMovement();
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        // Move velocity by key input
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        if (moveDirection.sqrMagnitude > 0.0001f) moveDirection.Normalize();

        // Initial horizontal velocity (mainly by gravity)
        Vector3 targetHorizontalVelocity = moveDirection * currentSpeed;

        Vector3 finalVerticalVelocity;

        // Start jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            finalVerticalVelocity = transform.up * jumpForce;
        }

        // Holding jump
        else if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                finalVerticalVelocity = transform.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                finalVerticalVelocity = GetGravityVelocity();
            }
        }

        // Not jumping
        else
        {
            isJumping = false;
            finalVerticalVelocity = GetGravityVelocity();
        }

        rb.linearVelocity = targetHorizontalVelocity + finalVerticalVelocity;
    }

    Vector3 GetGravityVelocity()
    {
        float currentVerticalSpeed = Vector3.Dot(rb.linearVelocity, transform.up);
        return transform.up * currentVerticalSpeed;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        float normalizedZRotation = zRotation % 360f;

        float adjustedMouseX = mouseX;
        float adjustedMouseY = mouseY;
        switch (viewState)
        {
            case ViewState.Default:
                adjustedMouseX = mouseX; adjustedMouseY = mouseY; break;
            case ViewState.Left:
                adjustedMouseX = -mouseY; adjustedMouseY = mouseX; break;
            case ViewState.UpDown:
                adjustedMouseX = -mouseX; adjustedMouseY = -mouseY; break;
            case ViewState.Right:
                adjustedMouseX = mouseY; adjustedMouseY = -mouseX; break;
        }

        yRotation += adjustedMouseX;
        xRotation -= adjustedMouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
    }

    void HandleViewRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int s = ((int)viewState + 3) % 4;
            viewState = (ViewState)s;
            zRotation = s * 90f;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            int s = ((int)viewState + 1) % 4;
            viewState = (ViewState)s;
            zRotation = s * 90f;
        }
    }
}