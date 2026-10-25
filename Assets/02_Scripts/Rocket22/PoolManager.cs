using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;

    public static PoolManager Instance { get { return _instance; } }
    
    [SerializeField] private int _poolSize;
    [SerializeField] private GameObject _poolPrefab;

    private Queue<GameObject> _prefabs;

    private void Awake() => SetSingleton();

    private void Start() => FillPool();
    
    private void FillPool()
    {
        _prefabs = new Queue<GameObject>();

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject go = Instantiate(_poolPrefab);
            go.transform.SetParent(transform);
            go.SetActive(false);
            
            _prefabs.Enqueue(go);
        }
    }

    public GameObject Take()
    {
        if (_prefabs.Count > 0)
        {
            GameObject go = _prefabs.Dequeue();
            go.SetActive(true);

            return go;
        }

        return null;
    }

    public void Return(GameObject go)
    {
        if (_prefabs.Count > _poolSize)
        {
            return;
        }
        
        go.SetActive(false);
        _prefabs.Enqueue(go);
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
