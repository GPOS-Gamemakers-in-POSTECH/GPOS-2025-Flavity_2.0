using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float groundDrag = 5f;
    
    [Header("마우스 시점 설정")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;
    
    private Rigidbody rb;
    private float xRotation = 0f;
    private float yRotation = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        
        // 초기 회전값 설정
        xRotation = transform.eulerAngles.x;
        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        
        // 게임 중 마우스 커서 풀기 (ESC 키)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
    void HandleMovement()
    {
        // WASD 키 입력 감지
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        if (Input.GetKey(KeyCode.W))
            verticalInput += 1f;
        if (Input.GetKey(KeyCode.S))
            verticalInput -= 1f;
        if (Input.GetKey(KeyCode.D))
            horizontalInput += 1f;
        if (Input.GetKey(KeyCode.A))
            horizontalInput -= 1f;
        
        // 입력 방향으로 움직임 (카메라 기준)
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        moveDirection.Normalize();
        
        // 속도 적용
        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.linearVelocity.y,
            moveDirection.z * moveSpeed
        );
        
        // 마찰력 적용
        rb.linearDamping = horizontalInput != 0 || verticalInput != 0 ? 0 : groundDrag;
    }
    
    void HandleMouseLook()
    {
        // 마우스 입력 감지
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 좌우 회전 (Y축)
        yRotation += mouseX;
        
        // 상하 회전 (X축) - 제한 적용
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        // 오브젝트 회전 적용
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
