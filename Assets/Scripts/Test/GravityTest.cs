using UnityEngine;
using UnityEngine.InputSystem;

public class GravityTest : MonoBehaviour
{
    [SerializeField] private InputActionReference _inputActionReference;
    private InputAction _gravitySwitchAction;

#region life_cycle
    private void Awake()
    {
        if (_inputActionReference != null)
        {
            _gravitySwitchAction = _inputActionReference.action;
        }
        else
        {
            Debug.LogError("Input Action Reference가 할당되지 않았습니다.", this);
            return;
        }

        _gravitySwitchAction.performed += ChangeGravity;
    }

    private void OnEnable()
    {
        _gravitySwitchAction?.Enable();
    }

    private void OnDisable()
    {
        _gravitySwitchAction?.Disable();
    }

    private void OnDestroy()
    {
        if (_gravitySwitchAction != null)
        {
            _gravitySwitchAction.performed -= ChangeGravity;
        }
    }
#endregion

    private void ChangeGravity(InputAction.CallbackContext context)
    {
        MyDebug.Log("asdf");
        switch (GravitySwitcher.Instance.CurrentGravityDirection)
        {
            case GravityDirection.SOUTH:
                GravitySwitcher.Instance.SwitchGravity(GravityDirection.EAST);
                break;
            case GravityDirection.EAST:
                GravitySwitcher.Instance.SwitchGravity(GravityDirection.NORTH);
                break;
            case GravityDirection.NORTH:
                GravitySwitcher.Instance.SwitchGravity(GravityDirection.WEST);
                break;
            case GravityDirection.WEST:
                GravitySwitcher.Instance.SwitchGravity(GravityDirection.SOUTH);
                break;
            default:
                Debug.LogWarning("정의되지 않은 중력 방향입니다: " + GravitySwitcher.Instance.CurrentGravityDirection);
                break;
        }
    }
}