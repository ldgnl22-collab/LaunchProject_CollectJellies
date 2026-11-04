using GravityCollect;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 6f;
    [SerializeField] private LayerMask _groundMask;

    [Header("Touch Settings")]
    [SerializeField] private float _swipeDistance = 70f;
    [SerializeField] private float _rotationSpeed = 1000f;


    private SlimeMotion _motion;
    private Transform _transform;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private Vector2 _firstPosition;
    private int _fingerId = -1;
    private bool _isGestureResolved;

    private Quaternion _targetRotation;

    private void Awake()
    {
        CacheComponent();
        _targetRotation = _transform.rotation;
        _rigidbody.freezeRotation = true;
        _motion.useClipHop = false;
    }

    private void Start()
    {
        _motion.SetSpeed(_moveSpeed);
    }

    private void Update()
    {
        OnTouch();
        Rotate();
        Move();
    }

    private void OnTouch()
    {
        if (Input.touchCount == 0)
        {
            PositionReset();
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (_fingerId == -1)
                {
                    _fingerId = touch.fingerId;
                    _firstPosition = touch.position;
                    _isGestureResolved = false;
                }

                continue;
            }

            if (touch.fingerId != _fingerId)
            {
                continue;
            }

            if (touch.phase == TouchPhase.Moved &&
                !_isGestureResolved)
            {
                Vector2 move = touch.position - _firstPosition;

                if (Mathf.Abs(move.x) >= _swipeDistance &&
                    Mathf.Abs(move.x) > Mathf.Abs(move.y))
                {
                    if (move.x > 0f)
                    {
                        RightRotate();
                    }
                    else
                    {
                        LeftRotate();
                    }

                    _isGestureResolved = true;
                }
                else if (Mathf.Abs(move.y) >= _swipeDistance)
                {
                    _isGestureResolved = true;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (!_isGestureResolved)
                {
                    Vector2 move = touch.position - _firstPosition;

                    if (move.magnitude < _swipeDistance)
                    {
                        Jump();
                    }
                    else if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                    {
                        if (move.x > 0f)
                        {
                            RightRotate();
                        }
                        else
                        {
                            LeftRotate();
                        }
                    }
                }

                PositionReset();
                return;
            }
            else if (touch.phase == TouchPhase.Canceled)
            {
                PositionReset();
                return;
            }
        }
    }

    private void PositionReset()
    {
        _fingerId = -1;
        _firstPosition = Vector2.zero;
        _isGestureResolved = false;
    }

    private bool IsGrounded()
    {
        float checkDistance = _collider.bounds.extents.y + 0.1f;

        return Physics.Raycast(_collider.bounds.center, Vector3.down, checkDistance, _groundMask);
    }

    private void Jump()
    {
        if (IsGrounded() == false)
        {
            return;
        }

        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _motion.Jump();
    }

    private void LeftRotate()
    {
        _targetRotation *= Quaternion.Euler(0f, -90f, 0f);
    }

    private void RightRotate()
    {
        _targetRotation *= Quaternion.Euler(0f, 90f, 0f);
    }

    private void Rotate()
    {
        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation,
            _targetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }

    private void Move()
    {
        Vector3 direction = _targetRotation * Vector3.forward;
        _transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    private void CacheComponent()
    {
        _motion = GetComponent<SlimeMotion>();
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }
}