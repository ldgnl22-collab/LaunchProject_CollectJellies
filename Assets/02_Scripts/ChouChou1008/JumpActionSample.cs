using GravityCollect;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpActionSample : MonoBehaviour
{
    [SerializeField] private InputAction _jumpAction;
    private SlimeMotion _motion;

    private void Awake() => CacheComponent();
    private void OnEnable()
    {
        _jumpAction.Enable();
        BindInputEvent();
    }

    private void OnDisable()
    {
        _jumpAction.Disable();
        UnbindInputEvent();
    }

    private void BindInputEvent()
    {
        _jumpAction.performed += OnJump;
    }

    private void UnbindInputEvent()
    {
        _jumpAction.performed -= OnJump;
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        _motion.Jump();
    }

    private void CacheComponent()
    {
        _motion =  GetComponent<SlimeMotion>();
    }
    
    
}
