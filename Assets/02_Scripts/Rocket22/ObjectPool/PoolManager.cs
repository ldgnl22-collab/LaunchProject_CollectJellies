using System;
using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get ; private set; }
    
    // 직렬화 된 private 프리팹 변수
    
    [SerializeField] JellyManager _object;
    // public ObjectPool<객체> _객체명;
    public ObjectPool<JellyManager> _objectPool{get; private set;}
    
    private void Awake()
    {
        SetSingleton();
    }

    private void Start()
    {
        InitPoolCreate();
    }

    // ObjectPool 초기화
    private void InitPoolCreate()
    {
        _objectPool = new ObjectPool<JellyManager>(_object, 20, this.transform);
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
