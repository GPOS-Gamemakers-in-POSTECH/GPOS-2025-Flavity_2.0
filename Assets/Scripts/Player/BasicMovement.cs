using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    [Header("Input 참조")]
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxJumpTime = 0.3f;
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("마우스 시점 설정")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("카메라")]
    [SerializeField] private Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;

    private GravityDirection CurrentGravity => GravitySwitcher.Instance.CurrentGravityDirection;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;

    // --- 내부 캐싱 변수 (InputHandler와 결합도 낮춤) ---
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
            // 값 변경 이벤트 구독
            inputHandler.OnMove += UpdateMoveInput;
            inputHandler.OnLook += UpdateLookInput;
            inputHandler.OnSprint += UpdateSprintState;
            inputHandler.OnJump += UpdateJumpState;

            // 트리거 이벤트 구독
            inputHandler.OnJumpTriggered += HandleJumpTriggered;
            inputHandler.OnPauseTriggered += HandleUnlockCursor;
            inputHandler.OnGravityChange += ChangeGravityState;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            // 구독 해제
            inputHandler.OnMove -= UpdateMoveInput;
            inputHandler.OnLook -= UpdateLookInput;
            inputHandler.OnSprint -= UpdateSprintState;
            inputHandler.OnJump -= UpdateJumpState;

            inputHandler.OnJumpTriggered -= HandleJumpTriggered;
            inputHandler.OnPauseTriggered -= HandleUnlockCursor;
            inputHandler.OnGravityChange -= ChangeGravityState;
        }
    }

    void Update()
    {
        CheckGround();
        HandleMouseLook();
        HandleMovement();
    }

    // --- 이벤트 핸들러 (내부 상태 갱신) ---

    private void UpdateMoveInput(Vector2 input) => _currentMoveInput = input;
    private void UpdateLookInput(Vector2 input) => _currentLookInput = input;
    private void UpdateSprintState(bool isSprinting) => _isSprinting = isSprinting;
    private void UpdateJumpState(bool isHeld)
    {
        _isJumpHeld = isHeld;
        if (!isHeld) isJumping = false; // 버튼 떼면 점프 상승 중단
    }

    private void HandleJumpTriggered()
    {
        if (isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
        }
    }

    private void HandleUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
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

    // --- 메인 로직 ---

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }

    public void AlignPlayerToGravity()
    {
        if (GravitySwitcher.Instance == null) return;
        // (기존 중력 정렬 로직 유지)
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

        // 1. 좌우 회전 (플레이어 전체 회전)
        transform.Rotate(Vector3.up * mouseX);

        // 2. 위아래 회전값 계산
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // 3. (추가된 부분) 계산된 xRotation을 카메라에 적용합니다.
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        Vector3 localMoveDir = new Vector3(_currentMoveInput.x, 0f, _currentMoveInput.y).normalized;

        float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;

        // 2. 현재 월드 속도를 로컬 속도로 변환합니다.
        // 이렇게 하면 localVelocity.y는 항상 "캐릭터 기준 수직 속도(중력 방향)"가 됩니다.
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float currentVerticalSpeed = localVelocity.y;

        // 3. 점프 로직 (로컬 Y축 제어)
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

        // 4. 최종 로컬 속도 조합
        // X, Z는 입력값(속력 적용)으로 덮어쓰고, Y(중력/점프)는 유지합니다.
        Vector3 finalLocalVelocity = new Vector3(
            localMoveDir.x * currentSpeed,
            currentVerticalSpeed,
            localMoveDir.z * currentSpeed
        );

        // 5. 로컬 속도를 다시 월드 속도로 변환하여 적용
        // TransformDirection이 현재 캐릭터의 회전(중력 방향)을 반영하여 월드 좌표로 바꿔줍니다.
        rb.linearVelocity = transform.TransformDirection(finalLocalVelocity);
    }
}