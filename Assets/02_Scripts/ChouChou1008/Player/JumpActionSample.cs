using GravityCollect;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpActionSample : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction _jumpAction;            // 테스트용 <Keyboard>/space
    
    [Header("Jump Setting")]
    [SerializeField] private float _jumpHeight = 1.7f;           // 실제로 뜨는 높이 (m)
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.7f;
    
    private Rigidbody _rigidbody;
    private Collider _collider;
    private SlimeMotion _motion;
    private JellyTrail _trail;
    private float _jumpTimer;
    
    public bool IsGrounded { get; private set; }
    public bool IsJumping => _jumpTimer > 0f;

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
        if (_jumpTimer > 0f)
        {
            _jumpTimer -= Time.fixedDeltaTime;
        }
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

        Vector3 up = transform.up;
        float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * _jumpHeight);

        _rigidbody.linearVelocity += up * jumpSpeed;
        IsGrounded = false;
        _jumpTimer = 2f * jumpSpeed / Physics.gravity.magnitude + 0.1f;
        

        _motion.Jump();
        _trail.RecordJump();
    }

    private bool CheckGround()
    {
        Vector3 origin = _collider.bounds.center;
        Debug.DrawRay(origin, -transform.up, Color.red);
        return Physics.Raycast(origin, -transform.up, _groundCheckDistance, _groundLayer);
    }

    private void CacheComponent()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _motion = GetComponent<SlimeMotion>();
        _trail = GetComponent<JellyTrail>();
    }
}