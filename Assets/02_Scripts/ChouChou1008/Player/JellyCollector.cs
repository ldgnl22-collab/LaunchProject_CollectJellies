using System;
using GravityCollect;
using UnityEngine;

public class JellyCollector : MonoBehaviour
{
    [Header("Item Check")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _checkDistance = 1.2f;
    
    [Header("Fall Check")]
    [SerializeField] private float _dieFallHeight = 3f;   // 이만큼 떨어지면 사망
    
    private SlimeMotion _motion;
    private JumpActionSample _jump;
    private Rigidbody _rigidbody;
    private float _lastGroundY;
    
    public event Action OnCollected;
    public event Action OnReturn;


    private void Awake() => CacheComponent();
    
    private void Update()
    {
        if (_motion.IsDead)
        {
            return;
        }
        
        CheckItem();
        CheckFall();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_motion.IsDead)
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Trap"))
        {
            OnReturn?.Invoke();
        }
    }

    private void CheckItem()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _checkDistance, _layerMask))
        {
            Destroy(hit.transform.gameObject);
            OnCollected?.Invoke();
        }
    }

    private void CheckFall()
    {
        if (_jump.IsGrounded)
        {
            _lastGroundY = transform.position.y;
            return;
            // 땅에 있으면 지금 높이 기억
        }

        if (transform.position.y < _lastGroundY - _dieFallHeight)
        {
            _motion.Die();
            _rigidbody.isKinematic = true;
            // 기억한 높이보다 많이 떨어지면 사망, 그 자리에 멈춤
        }
    }

    private void CacheComponent()
    {
        _motion = GetComponent<SlimeMotion>();
        _jump = GetComponent<JumpActionSample>();
        _rigidbody = GetComponent<Rigidbody>();
        _lastGroundY = transform.position.y;
    }
}