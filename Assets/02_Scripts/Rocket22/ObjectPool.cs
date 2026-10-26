using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    [Serializable]
    public class PoolData
    {
        public PoolType Key;
        public T Prefab;
        public int Size;
    }
    
    private static ObjectPool<T> _instance;

    public static ObjectPool<T> Instance { get { return _instance; } }

    private Dictionary<PoolType, Queue<T>> _dicPools
        = new Dictionary<PoolType, Queue<T>>();
    
    [SerializeField] private List<PoolData> _poolDataList;

    private void Awake()
    {
        SetSingleton();
    }

    private void Start()
    {
        // InitPool();
    }

    public void InitPool(T prefab)
    {
        for (int i = 0; i < _poolDataList.Count; i++)
        {
            PoolData data = _poolDataList[i];
            Queue<T> prefabs = new Queue<T>();

            GameObject obj = new GameObject($"{data.Key}");
            obj.transform.SetParent(transform);
            
            for (int j = 0; j < data.Size; j++)
            {
                T go = Instantiate(prefab);
                go.transform.SetParent(obj.transform);
                go.gameObject.SetActive(false);
                prefabs.Enqueue(go);
            }
            
            _dicPools.Add(data.Key, prefabs);
        }
    }

    public T Take(PoolType poolType)
    {
        if (_dicPools[poolType].Count > 0)
        {
            T go = _dicPools[poolType].Dequeue();
            go.gameObject.SetActive(true);

            if (go.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnTakePool();
            }

            return go;
        }

        return null;
    }

    public void Return(PoolType poolType, T obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.ReturnToPool();
        }
        
        obj.gameObject.SetActive(false);
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
