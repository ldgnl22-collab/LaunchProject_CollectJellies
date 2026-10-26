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

    private void InitPoolCreate()
    {
        // 풀 변수 = new ObjectPool<객체>(프리팹 변수, 풀 개수, 부모오브젝트 위치);
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
