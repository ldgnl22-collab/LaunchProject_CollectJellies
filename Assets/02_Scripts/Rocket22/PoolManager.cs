using System;
using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get ; private set; }
    
    // 직렬화 된 private 프리팹 변수
    
    
    // public ObjectPool<객체> _객체명;
    
    
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
