using UnityEngine;
using UnityEngine.InputSystem;

public class RotateAction : MonoBehaviour
{
    [SerializeField] private InputAction _RotateAction;

    private void OnEnable()
    {
       _RotateAction.Enable();
       BindInputEvent();
    }

    private void OnDisable()
    {
        _RotateAction.Disable();
        UnbindInputEvent();
    }

    private void BindInputEvent()
    {
        
    }

    private void UnbindInputEvent()
    {
        
    }
    
    
    
}
