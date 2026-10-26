using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private Queue<T> _pool = new Queue<T>();
    private T _prefab;
    private Transform _parent;

    public ObjectPool(T prefab, int initSize, Transform parent)
    {
        this._prefab = prefab;
        this._parent = parent;
        
        GameObject root = new GameObject($"{_prefab.name}");
        root.transform.SetParent(_parent);
        
        for (int i = 0; i < initSize; i++)
        {
            T obj = Object.Instantiate(_prefab, root.transform);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T TakePool()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);

            obj.OnTakePool();

            return obj;
        }

        return null;
    }

    public void ReturnPool(T obj)
    {
        obj.OnReturnPool();

        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
