using UnityEngine;

public class JellySpawner : MonoBehaviour 
{
    [SerializeField] private int _maxSlime = 30;
    
    private JellyCollector _jellyCollector;
    private JellyTrail _trail;
    
    
    private void Awake() => CacheComponent();

    private void OnEnable()
    {
        _jellyCollector.OnCollected += Spawn;
    }

    private void OnDisable()
    {
        _jellyCollector.OnCollected -= Spawn;
    }

    private void Spawn()
    {
        if (_trail.FollowerCount >= _maxSlime)
        {
            return;
        }
        
        JellyManager slime = PoolManager.Instance._objectPool.TakePool();
        if (slime == null)
        {
            return;
        }

        _trail.AddFollower(slime.Follower);
    }

    private void CacheComponent()
    {
        _jellyCollector= GetComponent<JellyCollector>();
        _trail = GetComponent<JellyTrail>();
    }
}