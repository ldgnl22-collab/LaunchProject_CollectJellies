using GravityCollect;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _gravity = 9.8f;
    [SerializeField] private LayerMask _UpRotation;
    [SerializeField] private LayerMask _UpExit;
    [SerializeField] private LayerMask _DownRotation;
    [SerializeField] private LayerMask _DownExit;
    [SerializeField] private LayerMask _OtherExit;
    
    private bool isWallCling;
    private Rigidbody _playerRigidbody;
    private SlimeMotion _motion;
    private RotateAction _rotateAction;
    public bool IsWallCling => isWallCling;

    private void Awake()
    {
        CacheComponent();
        Physics.gravity = Vector3.down * _gravity; //중력 최기화
    } 

    private void Start()
    {
        _motion.SetSpeed(_moveSpeed);
    }

    private void FixedUpdate()
    {
        Physics.gravity = -transform.up * _gravity;
        Move();
    }

    private void Move()
    {
        if (_playerRigidbody.isKinematic)
        {
            return; 
        }
        Vector3 velocity = _playerRigidbody.linearVelocity;

        if (_motion.IsDead)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            if (isWallCling)
            {
                velocity.y = 0f;
            }
            
            _playerRigidbody.linearVelocity = velocity;
            return;
            // 죽으면 앞으로 안 감
        }

        if (isWallCling)
        {
            WallMove();
        }
        else
        {
            Vector3 forward = _rotateAction.TargetRotation * Vector3.forward;
            velocity.x = forward.x * _moveSpeed;
            velocity.z = forward.z * _moveSpeed;
            _playerRigidbody.linearVelocity = velocity;  
        }
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (_motion.IsDead)
        {
            return;
        }
        if (!isWallCling && (_UpRotation.value &(1 << other.gameObject.layer)) !=0)
        {
            WallUpCling();
        }
        else if (!isWallCling && (_DownRotation.value & (1 << other.gameObject.layer)) != 0)
        {
            WallDownCling();
        }
        else if((isWallCling && (_OtherExit.value & (1 << other.gameObject.layer)) != 0))
        {
            OtherExit();
        }
        else if (isWallCling && (_UpExit.value & (1 << other.gameObject.layer)) != 0)
        {
            WallUpExit();
        }
        else if (isWallCling && (_DownExit.value & (1 << other.gameObject.layer)) != 0)
        {
            WallDownExit();
        }
    }

    private void WallUpCling()
    {
        isWallCling = true;
        _playerRigidbody.linearVelocity = Vector3.zero;
        Quaternion rotation = _rotateAction.TargetRotation * Quaternion.Euler(-90f, 0f, 0f);
        _rotateAction.SetTargetRotation(rotation); 
       // Physics.gravity = rotation * Vector3.down * _gravity; //플레이어 기준 아래로 중력 적용
    }

    private void WallDownCling()
    {
        isWallCling = true;
        _playerRigidbody.linearVelocity = Vector3.zero;
        Quaternion rotation = _rotateAction.TargetRotation * Quaternion.Euler(90f, 0f, 0f);
        _rotateAction.SetTargetRotation(rotation);
        //Physics.gravity = rotation * Vector3.down * _gravity;
    }

    private void WallUpExit()
    {
        isWallCling = false;
        _playerRigidbody.linearVelocity = Vector3.zero;
        Quaternion rotation = _rotateAction.TargetRotation * Quaternion.Euler(-90f, 0f, 0f);
        _rotateAction.SetTargetRotation(rotation);
        //Physics.gravity = rotation * Vector3.down * _gravity;
    }

    private void WallDownExit()
    {
        isWallCling = false;
        _playerRigidbody.linearVelocity = Vector3.zero;
        Quaternion rotation = _rotateAction.TargetRotation * Quaternion.Euler(90f, 0f, 0f);
        _rotateAction.SetTargetRotation(rotation);
        //Physics.gravity = rotation * Vector3.down * _gravity;
    }

    private void OtherExit()
    {
        isWallCling = false;
        _playerRigidbody.linearVelocity = Vector3.zero;
        Quaternion rotation = _rotateAction.TargetRotation * Quaternion.Euler(90f, 0f, 0f);
        _rotateAction.SetTargetRotation(rotation);
        //Physics.gravity = rotation * Vector3.down * _gravity;
    }

   
    
    private void WallMove()
    {
        Vector3 forward = _rotateAction.TargetRotation * Vector3.forward;
        Vector3 up = _rotateAction.TargetRotation * Vector3.up;
        float upSpeed = Vector3.Dot(_playerRigidbody.linearVelocity, up);

        _playerRigidbody.linearVelocity = forward * _moveSpeed + up * upSpeed;
    }

    private void CacheComponent()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _motion = GetComponent<SlimeMotion>();
        _rotateAction = GetComponent<RotateAction>();
    }
}