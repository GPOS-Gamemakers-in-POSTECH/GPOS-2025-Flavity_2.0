using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Animation Smoothing")]
    [Tooltip("Smoothing time for locomotion blending")]
    [SerializeField] private float dampTimeLocomotion = 0.15f;
    [Tooltip("Smoothing time for turning blending")]
    [SerializeField] private float dampTimeTurning = 0.1f;
    [Tooltip("Smoothing time for aiming blending")]
    [SerializeField] private float dampTimeAiming = 0.1f;

    [Header("Animation Rates (Settings)")]
    [SerializeField] private float aimSpeedMultiplier = 1.0f;
    [SerializeField] private float playRateHolster = 1.0f;
    [SerializeField] private float playRateUnholster = 1.0f;
    [SerializeField] private float playRateLocomotion = 1.0f;
    [SerializeField] private float playRateLocomotionFwd = 1.0f;
    [SerializeField] private float playRateLocomotionBack = 1.0f;
    [SerializeField] private float playRateLocomotionSide = 1.0f;

    [Header("IK Weights (Settings)")]
    [Range(0, 1)] [SerializeField] private float alphaBreathing = 1.0f;
    [Range(0, 1)] [SerializeField] private float alphaIKHandLeft = 1.0f;
    [Range(0, 1)] [SerializeField] private float alphaIKHandRight = 1.0f;

    // --- Internal State ---
    private Vector2 _currentMoveInput;
    private Vector2 _currentLookInput;
    private bool _isSprinting;
    private bool _isAiming; 

    // --- Animator Parameter Hashes ---
    // 1. Bool Parameters
    private static readonly int RunningHash = Animator.StringToHash("Running");
    private static readonly int HolsteredHash = Animator.StringToHash("Holstered");
    private static readonly int BoltActionHash = Animator.StringToHash("Bolt Action");
    private static readonly int AimHash = Animator.StringToHash("Aim");
    private static readonly int CrouchingHash = Animator.StringToHash("Crouching");
    private static readonly int ReloadingHash = Animator.StringToHash("Reloading");
    private static readonly int LoweredHash = Animator.StringToHash("Lowered");
    private static readonly int LeaningHash = Animator.StringToHash("Leaning");

    // [New] Trigger Parameters (Assuming 'Fire' trigger exists in Animator, check exact name!)
    private static readonly int FireHash = Animator.StringToHash("Fire"); 

    // 2. Float Parameters (Inputs)
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int MovementHash = Animator.StringToHash("Movement");
    private static readonly int TurningHash = Animator.StringToHash("Turning");
    private static readonly int LeaningInputHash = Animator.StringToHash("Leaning Input");
    private static readonly int LeaningForwardHash = Animator.StringToHash("Leaning Forward");
    private static readonly int AimingHash = Animator.StringToHash("Aiming");

    // 3. Float Parameters (Rates & Multipliers)
    private static readonly int AimingSpeedMultiplierHash = Animator.StringToHash("Aiming Speed Multiplier");
    private static readonly int PlayRateHolsterHash = Animator.StringToHash("Play Rate Holster");
    private static readonly int PlayRateUnholsterHash = Animator.StringToHash("Play Rate Unholster");
    private static readonly int PlayRateLocomotionHash = Animator.StringToHash("Play Rate Locomotion");
    private static readonly int PlayRateLocomotionForwardHash = Animator.StringToHash("Play Rate Locomotion Forward");
    private static readonly int PlayRateLocomotionBackwardsHash = Animator.StringToHash("Play Rate Locomotion Backwards");
    private static readonly int PlayRateLocomotionSidewaysHash = Animator.StringToHash("Play Rate Locomotion Sideways");

    // 4. Float Parameters (Alphas / Weights)
    private static readonly int AlphaBreathingHash = Animator.StringToHash("Alpha Breathing");
    private static readonly int AlphaIKHandLeftHash = Animator.StringToHash("Alpha IK Hand Left");
    private static readonly int AlphaIKHandRightHash = Animator.StringToHash("Alpha IK Hand Right");
    private static readonly int AlphaActionOffsetHash = Animator.StringToHash("Alpha Action Offset");

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_inputHandler == null) _inputHandler = GetComponentInParent<PlayerInputHandler>();
        if (_playerMovement == null) _playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Start()
    {
        // Apply initial settings
        UpdateAnimationRates();
        UpdateIKWeights();
    }

    private void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnMove += HandleMoveInput;
            _inputHandler.OnLook += HandleLookInput;
            _inputHandler.OnSprint += HandleSprintInput;
            _inputHandler.OnLean += HandleLeanInput;
            _inputHandler.OnReload += HandleReloadInput;
            _inputHandler.OnAim += HandleAimInput; 
            _inputHandler.OnAttack += HandleAttackInput; // [New] Subscribe to Attack
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnMove -= HandleMoveInput;
            _inputHandler.OnLook -= HandleLookInput;
            _inputHandler.OnSprint -= HandleSprintInput;
            _inputHandler.OnLean -= HandleLeanInput;
            _inputHandler.OnReload -= HandleReloadInput;
            _inputHandler.OnAim -= HandleAimInput;
            _inputHandler.OnAttack -= HandleAttackInput; // [New] Unsubscribe
        }
    }

    private void Update()
    {
        UpdateLocomotion();
        UpdateTurning();
        UpdateAiming(); 
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _animator != null)
        {
            UpdateAnimationRates();
            UpdateIKWeights();
        }
    }

    // --- Event Handlers ---

    private void HandleMoveInput(Vector2 input) => _currentMoveInput = input;
    private void HandleLookInput(Vector2 input) => _currentLookInput = input;
    private void HandleSprintInput(bool sprintState)
    {
        _isSprinting = sprintState;
        _animator.SetBool(RunningHash, _isSprinting);
    }

    private void HandleLeanInput(float leanValue)
    {
        SetLeaningInput(leanValue);
        bool isLeaning = Mathf.Abs(leanValue) > 0.05f;
        SetLeaning(isLeaning);
    }

    private void HandleReloadInput()
    {
        SetReloading(true);
    }
    
    public void HandleAimInput(bool aimState)
    {
        _isAiming = aimState;
        SetAim(aimState);
    }

    // [New] Handle Attack Input (Left Click)
    private void HandleAttackInput(bool attackState)
    {
        // Only fire on button down (true)
        if (attackState)
        {
            // Trigger the "Fire" animation
            // NOTE: Make sure your Animator has a Trigger parameter named "Fire"
            // If it's a bool, use SetBool(FireHash, true) and reset it later.
            _animator.SetTrigger(FireHash); 
        }
    }

    // --- Update Logic ---

    private void UpdateLocomotion()
    {
        // Force Movement to 1 if there is any input (fixes deadzone issue)
        float inputMagnitude = _currentMoveInput.magnitude;
        float targetMovement = inputMagnitude > 0.1f ? 1f : 0f;

        _animator.SetFloat(HorizontalHash, _currentMoveInput.x, dampTimeLocomotion, Time.deltaTime);
        _animator.SetFloat(VerticalHash, _currentMoveInput.y, dampTimeLocomotion, Time.deltaTime);
        _animator.SetFloat(MovementHash, targetMovement, dampTimeLocomotion, Time.deltaTime);
    }

    private void UpdateTurning()
    {
        float targetTurn = Mathf.Clamp(_currentLookInput.x, -1f, 1f);
        _animator.SetFloat(TurningHash, targetTurn, dampTimeTurning, Time.deltaTime);
    }

    private void UpdateAiming()
    {
        float targetAimAlpha = _isAiming ? 1.0f : 0.0f;
        _animator.SetFloat(AimingHash, targetAimAlpha, dampTimeAiming, Time.deltaTime);
    }

    private void UpdateAnimationRates()
    {
        _animator.SetFloat(AimingSpeedMultiplierHash, aimSpeedMultiplier);
        _animator.SetFloat(PlayRateHolsterHash, playRateHolster);
        _animator.SetFloat(PlayRateUnholsterHash, playRateUnholster);
        _animator.SetFloat(PlayRateLocomotionHash, playRateLocomotion);
        _animator.SetFloat(PlayRateLocomotionForwardHash, playRateLocomotionFwd);
        _animator.SetFloat(PlayRateLocomotionBackwardsHash, playRateLocomotionBack);
        _animator.SetFloat(PlayRateLocomotionSidewaysHash, playRateLocomotionSide);
    }

    private void UpdateIKWeights()
    {
        _animator.SetFloat(AlphaBreathingHash, alphaBreathing);
        _animator.SetFloat(AlphaIKHandLeftHash, alphaIKHandLeft);
        _animator.SetFloat(AlphaIKHandRightHash, alphaIKHandRight);
    }

    // --- Public Control Methods ---

    #region State Controls (Bools)
    public void SetHolstered(bool state) => _animator.SetBool(HolsteredHash, state);
    public void SetCrouching(bool state) => _animator.SetBool(CrouchingHash, state);
    public void SetReloading(bool state) => _animator.SetBool(ReloadingHash, state);
    public void SetAim(bool state) => _animator.SetBool(AimHash, state);
    public void SetLowered(bool state) => _animator.SetBool(LoweredHash, state);
    public void SetLeaning(bool state) => _animator.SetBool(LeaningHash, state);
    public void SetBoltAction(bool state) => _animator.SetBool(BoltActionHash, state);
    #endregion

    #region Value Controls (Floats)
    public void SetLeaningInput(float value) => _animator.SetFloat(LeaningInputHash, value);
    public void SetLeaningForward(float value) => _animator.SetFloat(LeaningForwardHash, value);
    public void SetAimingAlpha(float value) => _animator.SetFloat(AimingHash, value);
    
    public void SetLeftHandIK(float weight)
    {
        alphaIKHandLeft = weight;
        _animator.SetFloat(AlphaIKHandLeftHash, weight);
    }

    public void SetActionOffset(float value) => _animator.SetFloat(AlphaActionOffsetHash, value);
    #endregion
}