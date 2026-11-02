using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class Slime : MonoBehaviour
{
    [SerializeField] private int _jumpHight ;
    [SerializeField] private float _rotationSpeed= 1000f;
    
    private Transform _transform;
    private Vector2 _rotate;
    private Quaternion _targetRotation;
    

    private void Awake() => CacheComponent();
    
    private void Update()
    {
       // OnRotate();
    }
    
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"PlayerMover: Jump 단계는 {context.phase}입니다.");  
        }
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        _rotate = context.ReadValue<Vector2>();
        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation,
            _targetRotation,
            _rotationSpeed * Time.deltaTime
        );
        Debug.Log($"PlayerRotate: Rotate 단계는 {context.phase}입니다.");
    }
    private void CacheComponent()
    {
        _transform = GetComponent<Transform>();
    }
}
