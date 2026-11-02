using UnityEngine;

public class JellyManager : MonoBehaviour, IPoolable
{
    private JellyFollower _follower;
    
    public JellyFollower Follower => _follower;
    
    
    private void Awake() => _follower = GetComponent<JellyFollower>();
    
    
    public void OnTakePool()
    {
    }

    public void OnReturnPool()
    {
    }
}