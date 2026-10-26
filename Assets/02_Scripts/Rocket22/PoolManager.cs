using System;
using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public class PoolData
    {
        public PoolType Key;
        public GameObject Prefab;
        public int Size;
    }
    
    private static PoolManager _instance;

    public static PoolManager Instance { get { return _instance; } }

    private Dictionary<PoolType, Queue<GameObject>> _dicPools
        = new Dictionary<PoolType, Queue<GameObject>>();
    
    [SerializeField] private List<PoolData> _poolDataList;

    private void Awake()
    {
        SetSingleton();
    }

    private void Start()
    {
        InitPool();
    }

    private void InitPool()
    {
        for (int i = 0; i < _poolDataList.Count; i++)
        {
            PoolData data = _poolDataList[i];
            Queue<GameObject> prefabs = new Queue<GameObject>();

            GameObject obj = new GameObject($"{data.Key}");
            obj.transform.SetParent(transform);
            
            for (int j = 0; j < data.Size; j++)
            {
                GameObject go = Instantiate(data.Prefab);
                go.transform.SetParent(obj.transform);
                go.SetActive(false);
                prefabs.Enqueue(go);
            }
            
            _dicPools.Add(data.Key, prefabs);
        }
    }

    public GameObject Take(PoolType poolType)
    {
        if (_dicPools[poolType].Count > 0)
        {
            GameObject go = _dicPools[poolType].Dequeue();
            go.SetActive(true);

            if (go.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnTakePool();
            }

            return go;
        }

        return null;
    }

    public void Return(PoolType poolType, GameObject obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.ReturnToPool();
        }
        
        obj.SetActive(false);
        _dicPools[poolType].Enqueue(obj);
    }

    private void SetSingleton()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }
}
