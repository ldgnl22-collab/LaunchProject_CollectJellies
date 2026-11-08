using UnityEngine;
using UnityEngine.InputSystem;

public class RotateAction : MonoBehaviour
{
    [Header("Touch Setting")]
    [SerializeField] private InputAction _rotateAction;          // <Pointer>/press
    [SerializeField] private float _rotationSpeed = 1000f;
    [SerializeField] private float _swipDistance = 70f;
    
    private Transform _transform;
    private Pointer _pressPointer;
    private Vector2 _startDirection;
    private Vector2 _endDirection;
    private Vector2 _distance;
    private Quaternion _targetRotation;
    private JellyTrail _trail;
    private JumpActionSample _jump;

    public Quaternion TargetRotation => _targetRotation;
    

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
            _pressPointer = _rotateAction.activeControl?.device as Pointer;
            if (_pressPointer == null)
            {
                return;
            }
            _startDirection = _pressPointer.position.ReadValue();
        }

        if (_rotateAction.WasReleasedThisFrame() && _pressPointer != null)
        {
            _endDirection = _pressPointer.position.ReadValue();
            _distance = _endDirection - _startDirection;
            _pressPointer = null;
            
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
                _jump.Jump();
                //모바일 점프
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
    public void SetTargetRotation(Quaternion rotation)
    {
        _targetRotation = rotation;
    }
    
    private void CacheComponent()
    {
        _transform = transform;
        _targetRotation = _transform.rotation;
        _jump = GetComponent<JumpActionSample>();
    }
}