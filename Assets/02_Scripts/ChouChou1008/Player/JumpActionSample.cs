using GravityCollect;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpActionSample : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction _jumpAction;            // 테스트용 <Keyboard>/space
    
    [Header("Jump Setting")]
    [SerializeField] private float _jumpHeight = 1.5f;           // 실제로 뜨는 높이 (m)
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    
    private Rigidbody _rigidbody;
    private Collider _collider;
    private SlimeMotion _motion;
    private JellyTrail _trail;
    
    public bool IsGrounded { get; private set; }

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

    private void FixedUpdate()
    {
        IsGrounded = CheckGround();
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
        Jump();
    }

    public void Jump()
    {
        if (_motion.IsDead || !IsGrounded)
        {
            return;
            // 죽었거나 공중이면 점프 안 함
        }

        float jumpSpeed = Mathf.Sqrt(2f * -Physics.gravity.y * _jumpHeight);
        
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = jumpSpeed;
        _rigidbody.linearVelocity = velocity;
        IsGrounded = false;

        _motion.Jump();
        _trail.RecordJump();
    }

    private bool CheckGround()
    {
        Vector3 origin = transform.position;
        origin.y = _collider.bounds.min.y + 0.1f;
        // Collider 바닥에서 살짝 위
        return Physics.Raycast(origin, Vector3.down, 0.1f + _groundCheckDistance, _groundLayer);
    }

    private void CacheComponent()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _motion = GetComponent<SlimeMotion>();
        _trail = GetComponent<JellyTrail>();
    }
}