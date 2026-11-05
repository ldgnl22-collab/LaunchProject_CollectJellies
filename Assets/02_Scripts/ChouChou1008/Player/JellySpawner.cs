using UnityEngine;
using System.Collections.Generic;
using GravityCollect;

public class JellySpawner : MonoBehaviour 
{
    [SerializeField] private int _maxSlime = 30;
    
    private JellyCollector _jellyCollector;
    private JellyTrail _trail;
    private SlimeMotion _motion;
    
    private void Awake() => CacheComponent();

    private void OnEnable()
    {
        _jellyCollector.OnCollected += Spawn;
        _jellyCollector.OnReturn += Return;
    }

    private void OnDisable()
    {
        _jellyCollector.OnCollected -= Spawn;
        _jellyCollector.OnReturn -= Return;
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

    private void Return()
    {
        JellyFollower follower = _trail.RemoveLastFollower();

        if (follower == null)
        {
            _motion.Die();
            return;
            // 없앨 젤리가 없으면 플레이어 사망
        }

        JellyManager jelly = follower.GetComponent<JellyManager>();
        PoolManager.Instance._objectPool.ReturnPool(jelly);
    }

    private void CacheComponent()
    {
        _jellyCollector= GetComponent<JellyCollector>();
        _trail = GetComponent<JellyTrail>();
        _motion = GetComponent<SlimeMotion>();
    }
}