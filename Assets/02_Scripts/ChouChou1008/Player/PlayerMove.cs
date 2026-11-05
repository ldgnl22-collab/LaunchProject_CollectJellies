using GravityCollect;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    
    private Rigidbody _playerRigidbody;
    private SlimeMotion _motion;
    private RotateAction _rotateAction;
    
    private void Awake() => CacheComponent();

    private void Start()
    {
        _motion.SetSpeed(_moveSpeed);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 velocity = _playerRigidbody.linearVelocity;

        if (_motion.IsDead)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            _playerRigidbody.linearVelocity = velocity;
            return;
            // 죽으면 앞으로 안 감
        }

        Vector3 forward = _rotateAction.TargetRotation * Vector3.forward;
        velocity.x = forward.x * _moveSpeed;
        velocity.z = forward.z * _moveSpeed;
        _playerRigidbody.linearVelocity = velocity;
        // 앞뒤·좌우만 정하고, 위아래(y)는 점프·중력에 맡김
    }

    private void CacheComponent()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _motion = GetComponent<SlimeMotion>();
        _rotateAction = GetComponent<RotateAction>();
    }
}