using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Asset")]
    public InputActionAsset inputAsset;

    #region Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction gravityLeftAction;
    private InputAction gravityUpAction;
    private InputAction gravityRightAction;
    private InputAction pauseAction;
    #endregion

    #region Events
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action<bool> OnSprint;
    public event Action<bool> OnJump;
    public event Action OnJumpTriggered;
    public event Action OnPauseTriggered;
    public event Action<int> OnGravityChange;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (inputAsset == null)
        {
            MyDebug.LogError("Input Asset is not assigned in the Inspector.");
            return;
        }

        // 1. 모든 액션을 Input Asset에서 찾아서 할당합니다.
        InputActionMap playerMap = inputAsset.FindActionMap("Player");
        if (playerMap != null)
        {
            moveAction = playerMap.FindAction("Move");
            lookAction = playerMap.FindAction("Look");
            jumpAction = playerMap.FindAction("Jump");
            sprintAction = playerMap.FindAction("Sprint");

            // Gravity Actions (예시: Player Map에 있다고 가정)
            gravityLeftAction = playerMap.FindAction("GravityLeft");
            gravityUpAction = playerMap.FindAction("GravityUp");
            gravityRightAction = playerMap.FindAction("GravityRight");
        }
        else
        {
            MyDebug.LogError("Action Map 'Player' not found in Input Asset.");
        }

        // UI Action Map
        InputActionMap uiMap = inputAsset.FindActionMap("UI");
        if (uiMap != null)
        {
            pauseAction = uiMap.FindAction("Pause");
        }
        else
        {
            MyDebug.LogWarning("Action Map 'UI' not found. Pause functionality might be missing.");
        }
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(sprintAction);
        EnableAction(gravityLeftAction);
        EnableAction(gravityUpAction);
        EnableAction(gravityRightAction);
        EnableAction(pauseAction);

        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();

        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(sprintAction);
        DisableAction(gravityLeftAction);
        DisableAction(gravityUpAction);
        DisableAction(gravityRightAction);
        DisableAction(pauseAction);
    }
    #endregion

    #region Utility Methods
    private void EnableAction(InputAction action)
    {
        if (action != null && !action.enabled)
            action.Enable();
    }

    private void DisableAction(InputAction action)
    {
        if (action != null && action.enabled)
            action.Disable();
    }
    #endregion

    #region Event Binding
    private void BindEvents()
    {
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        if (lookAction != null)
        {
            lookAction.performed += OnLookPerformed;
            lookAction.canceled += OnLookCanceled;
        }

        if (sprintAction != null)
        {
            sprintAction.performed += OnSprintStarted;
            sprintAction.canceled += OnSprintEnded;
        }

        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpStarted;
            jumpAction.canceled += OnJumpEnded;
        }

        // UI
        if (pauseAction != null)
            pauseAction.performed += OnPausePerformed;

        // 중력 회전
        if (gravityLeftAction != null)
            gravityLeftAction.performed += OnGravityLeftPerformed;
        if (gravityUpAction != null)
            gravityUpAction.performed += OnGravityUpPerformed;
        if (gravityRightAction != null)
            gravityRightAction.performed += OnGravityRightPerformed;
    }

    private void UnbindEvents()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.canceled -= OnLookCanceled;
        }

        if (sprintAction != null)
        {
            sprintAction.performed -= OnSprintStarted;
            sprintAction.canceled -= OnSprintEnded;
        }

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpStarted;
            jumpAction.canceled -= OnJumpEnded;
        }

        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;

        if (gravityLeftAction != null)
            gravityLeftAction.performed -= OnGravityLeftPerformed;
        if (gravityUpAction != null)
            gravityUpAction.performed -= OnGravityUpPerformed;
        if (gravityRightAction != null)
            gravityRightAction.performed -= OnGravityRightPerformed;
    }
    #endregion

    #region Callback Methods
    private void OnMovePerformed(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => OnMove?.Invoke(Vector2.zero);

    private void OnLookPerformed(InputAction.CallbackContext ctx) => OnLook?.Invoke(ctx.ReadValue<Vector2>());
    private void OnLookCanceled(InputAction.CallbackContext ctx) => OnLook?.Invoke(Vector2.zero);

    private void OnSprintStarted(InputAction.CallbackContext ctx) => OnSprint?.Invoke(true);
    private void OnSprintEnded(InputAction.CallbackContext ctx) => OnSprint?.Invoke(false);

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        OnJumpTriggered?.Invoke();
        OnJump?.Invoke(true);
    }
    private void OnJumpEnded(InputAction.CallbackContext ctx) => OnJump?.Invoke(false);

    private void OnPausePerformed(InputAction.CallbackContext ctx) => OnPauseTriggered?.Invoke();
    private void OnGravityLeftPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(1);
    private void OnGravityUpPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(2);
    private void OnGravityRightPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(3);
    #endregion
}