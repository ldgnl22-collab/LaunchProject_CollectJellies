using GravityCollect;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    private Transform _transform;
    private Rigidbody _playerRigidbody;
    private SlimeMotion _motion;
    
    private void Awake() => CacheComponent();
    void Update()
    {
        Move();
        _motion.SetSpeed(_moveSpeed);
    }
    private void Move()
    {
        transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
    }
    private void CacheComponent()
    {
        _transform = transform;
        _playerRigidbody = GetComponent<Rigidbody>();
        _motion = GetComponent<SlimeMotion>();
    }
}
