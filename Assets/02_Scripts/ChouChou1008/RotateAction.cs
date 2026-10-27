using GravityCollect;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateAction : MonoBehaviour
{
    [Header("Touch Setting")]
    [SerializeField] private InputAction _rotateAction;
    [SerializeField] private float _rotationSpeed = 1000f;
    [SerializeField] private float _swipDistance = 70f;
    
    private Transform _transform;
    private Vector2 _startDirection;
    private Vector2 _endDirection;
    private Vector2 _distance;
    private Quaternion _targetRotation;
    

    private void Awake() => CacheComponent();

    private void Update()
    {
        CheckSlide();
        OnRotate();
    }
    private void OnEnable()
    {
        _rotateAction.Enable();
    }

    private void OnDisable()
    {
        _rotateAction.Disable();
    }

    private void CheckSlide()
    {
        if (_rotateAction.WasPressedThisFrame())
        {
            _startDirection =  Pointer.current.position.ReadValue();
        }

        if (_rotateAction.WasReleasedThisFrame())
        {
            _endDirection =  Pointer.current.position.ReadValue();
            _distance = _endDirection - _startDirection;
            if (_distance.x > _swipDistance)
            {
                RightRotate();
            }
            else if (_distance.x < -_swipDistance)
            {
                LeftRotate();
            }
            else
            {

            }
        }
    }
    
    
    private void OnRotate()
    {
        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation,
            _targetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }

    private void LeftRotate()
    {
        _targetRotation *= Quaternion.Euler(0f, -90f, 0f);
    }
    private void RightRotate()
    {
        _targetRotation *= Quaternion.Euler(0f, 90f, 0f);
    }
    
    private void CacheComponent(){
        _transform = transform;
        _targetRotation = _transform.rotation;

    }
    
}
