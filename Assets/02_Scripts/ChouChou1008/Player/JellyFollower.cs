using GravityCollect;
using UnityEngine;

public class JellyFollower : MonoBehaviour
{
    private JellyTrail _trail;
    private SlimeMotion _motion;
    private Transform _transform;
        
    private int _index;
    private float _lastDistance;
    
       
        
    private void Awake() => CacheComponent();

    public void Init(JellyTrail trail, int index)
    {
        _trail = trail;
        _index = index;
        _lastDistance = GetMyDistance();
        Follow();
    }

    public void Follow()
    {
        float distance = GetMyDistance();
            
        _trail.Sample(distance, out Vector3 position, out Quaternion rotation);
        _transform.SetPositionAndRotation(position, rotation);

        if (_trail.HasJump(_lastDistance, distance))
        {
            _motion.Jump();
        }

        _lastDistance = distance;
    }

    private float GetMyDistance()
    {
        return _trail.TotalDistance - _trail.Spacing * (_index + 1);
    }
    
    private void CacheComponent()
    {
        _motion = GetComponent<SlimeMotion>();
        _transform = transform;
    }
}