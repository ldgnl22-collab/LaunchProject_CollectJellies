using System;
using GravityCollect;
using UnityEngine;

public class JellyCollector : MonoBehaviour
{
    [Header("Item Check")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _checkDistance = 1.2f;

    [Header("Fall Check")]
    [SerializeField] private float _fallDelay = 0.5f;   // 땅이 없을 때 사망까지 기다리는 시간

    private SlimeMotion _motion;
    private JumpActionSample _jump;
    private Rigidbody _rigidbody;
    private RotateAction _rotateAction;
    private float _noGroundTime;

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
        Ray ray = new Ray(transform.position + transform.up * 0.5f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _checkDistance, _layerMask))
        {
            Destroy(hit.transform.gameObject);
            OnCollected?.Invoke();
        }
    }

    private void CheckFall()
    {
        bool isRotating = Quaternion.Angle(transform.rotation, _rotateAction.TargetRotation) > 1f;

        if (_jump.IsGrounded || _jump.IsJumping || isRotating)
        {
            _noGroundTime = 0f;
            return;
            // 땅에 있거나 점프, 회전 중이면 사망 안 함
        }

        _noGroundTime += Time.deltaTime;

        if (_noGroundTime >= _fallDelay)
        {
            _motion.Die();
            _rigidbody.isKinematic = true;
            // 땅이 없으면 사망, 그 자리에 멈춤
        }
    }

    private void CacheComponent()
    {
        _motion = GetComponent<SlimeMotion>();
        _jump = GetComponent<JumpActionSample>();
        _rigidbody = GetComponent<Rigidbody>();
        _rotateAction = GetComponent<RotateAction>();
    }
}