using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    // Instance of the auto-generated C# class
    private Flavity_InputSystem inputActions;

    #region Events
    // Movement & Look
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action<bool> OnSprint;
    public event Action<bool> OnJump;
    public event Action OnJumpTriggered;
    
    // Actions
    public event Action<float> OnLean; // -1(Left), 0(Neutral), 1(Right)
    public event Action OnReload;      // Reload Trigger
    public event Action<bool> OnAim;   // Aiming State (Right Click)
    public event Action<bool> OnAttack; // [New] Attack State (Left Click)
    
    // Gravity
    public event Action<int> OnGravityChange; // 1:Left, 2:Up, 3:Right

    // UI & System
    public event Action OnPauseTriggered;  // Game -> Pause (Player Map)
    public event Action OnCancelTriggered; // Menu -> Back (UI Map)
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Create a separate instance for this player
        inputActions = new Flavity_InputSystem();
    }

    private void OnEnable()
    {
        // Start in Player Mode
        SetInputMap(true);
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
        // Disable all maps
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }

    private void OnDestroy()
    {
        // Vital: Release memory when object is destroyed
        inputActions?.Dispose();
    }
    #endregion

    #region Map Control
    /// <summary>
    /// Switches between Player and UI input maps.
    /// </summary>
    /// <param name="isPlayerMode">true: Control Character, false: Control UI</param>
    public void SetInputMap(bool isPlayerMode)
    {
        if (isPlayerMode)
        {
            inputActions.UI.Disable();
            inputActions.Player.Enable();
            
            // Optional: Lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            inputActions.Player.Disable();
            inputActions.UI.Enable();
            
            // Optional: Unlock Cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    #endregion

    #region Event Binding
    private void BindEvents()
    {
        // --- Player Map ---
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Look.performed += OnLookPerformed;
        inputActions.Player.Look.canceled += OnLookCanceled;

        inputActions.Player.Sprint.performed += OnSprintStarted;
        inputActions.Player.Sprint.canceled += OnSprintEnded;

        inputActions.Player.Jump.performed += OnJumpStarted;
        inputActions.Player.Jump.canceled += OnJumpEnded;

        inputActions.Player.Lean.performed += OnLeanPerformed;
        inputActions.Player.Lean.canceled += OnLeanCanceled;

        inputActions.Player.Reload.performed += OnReloadPerformed;

        // Aim Binding
        inputActions.Player.Aim.performed += OnAimStarted;
        inputActions.Player.Aim.canceled += OnAimEnded;

        // [New] Attack Binding
        inputActions.Player.Attack.performed += OnAttackStarted;
        inputActions.Player.Attack.canceled += OnAttackEnded;

        inputActions.Player.GravityLeft.performed += OnGravityLeftPerformed;
        inputActions.Player.GravityUp.performed += OnGravityUpPerformed;
        inputActions.Player.GravityRight.performed += OnGravityRightPerformed;

        // Pause: Open Menu
        inputActions.Player.Pause.performed += OnPausePerformed;

        // --- UI Map ---
        // Cancel: Close Menu / Back (Esc key by default)
        inputActions.UI.Cancel.performed += OnCancelPerformed;
    }

    private void UnbindEvents()
    {
        // --- Player Map ---
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.Look.performed -= OnLookPerformed;
        inputActions.Player.Look.canceled -= OnLookCanceled;

        inputActions.Player.Sprint.performed -= OnSprintStarted;
        inputActions.Player.Sprint.canceled -= OnSprintEnded;

        inputActions.Player.Jump.performed -= OnJumpStarted;
        inputActions.Player.Jump.canceled -= OnJumpEnded;

        inputActions.Player.Lean.performed -= OnLeanPerformed;
        inputActions.Player.Lean.canceled -= OnLeanCanceled;

        inputActions.Player.Reload.performed -= OnReloadPerformed;

        // Aim Unbind
        inputActions.Player.Aim.performed -= OnAimStarted;
        inputActions.Player.Aim.canceled -= OnAimEnded;

        // [New] Attack Unbind
        inputActions.Player.Attack.performed -= OnAttackStarted;
        inputActions.Player.Attack.canceled -= OnAttackEnded;

        inputActions.Player.GravityLeft.performed -= OnGravityLeftPerformed;
        inputActions.Player.GravityUp.performed -= OnGravityUpPerformed;
        inputActions.Player.GravityRight.performed -= OnGravityRightPerformed;

        inputActions.Player.Pause.performed -= OnPausePerformed;

        // --- UI Map ---
        inputActions.UI.Cancel.performed -= OnCancelPerformed;
    }
    #endregion

    #region Callback Methods
    // Movement
    private void OnMovePerformed(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => OnMove?.Invoke(Vector2.zero);

    // Look
    private void OnLookPerformed(InputAction.CallbackContext ctx) => OnLook?.Invoke(ctx.ReadValue<Vector2>());
    private void OnLookCanceled(InputAction.CallbackContext ctx) => OnLook?.Invoke(Vector2.zero);

    // Actions
    private void OnSprintStarted(InputAction.CallbackContext ctx) => OnSprint?.Invoke(true);
    private void OnSprintEnded(InputAction.CallbackContext ctx) => OnSprint?.Invoke(false);

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        OnJumpTriggered?.Invoke();
        OnJump?.Invoke(true);
    }
    private void OnJumpEnded(InputAction.CallbackContext ctx) => OnJump?.Invoke(false);

    private void OnLeanPerformed(InputAction.CallbackContext ctx) => OnLean?.Invoke(ctx.ReadValue<float>());
    private void OnLeanCanceled(InputAction.CallbackContext ctx) => OnLean?.Invoke(0f);
    
    private void OnReloadPerformed(InputAction.CallbackContext ctx) => OnReload?.Invoke();

    // Aim Callbacks
    private void OnAimStarted(InputAction.CallbackContext ctx) => OnAim?.Invoke(true);
    private void OnAimEnded(InputAction.CallbackContext ctx) => OnAim?.Invoke(false);

    // [New] Attack Callbacks
    private void OnAttackStarted(InputAction.CallbackContext ctx) => OnAttack?.Invoke(true);
    private void OnAttackEnded(InputAction.CallbackContext ctx) => OnAttack?.Invoke(false);

    // Gravity & System
    private void OnPausePerformed(InputAction.CallbackContext ctx) => OnPauseTriggered?.Invoke();
    private void OnCancelPerformed(InputAction.CallbackContext ctx) => OnCancelTriggered?.Invoke();

    private void OnGravityLeftPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(1);
    private void OnGravityUpPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(2);
    private void OnGravityRightPerformed(InputAction.CallbackContext ctx) => OnGravityChange?.Invoke(3);
    #endregion
}