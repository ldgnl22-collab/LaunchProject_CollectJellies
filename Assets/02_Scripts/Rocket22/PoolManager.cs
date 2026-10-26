using System;
using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get ; private set; }
    
    // [SerializeField] private Bullet _bulletPrefab;
    
    // public ObjectPool<Bullet> _bulletPool;
    
    private void Awake()
    {
        SetSingleton();
    }

    private void Start()
    {
        InitPoolCreate();
    }

    private void InitPoolCreate()
    {
        // _bulletPool = new ObjectPool<Bullet>(_bulletPrefab, 10, transform);
    }

    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
