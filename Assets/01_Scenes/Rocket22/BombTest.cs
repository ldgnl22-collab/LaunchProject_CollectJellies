using UnityEngine;

public class BombTest : MonoBehaviour, IPoolable
{
    private float _elapsed;
    private Vector3 startPos;
    private void Update()
    {
        transform.Translate(Vector3.forward * 4 * Time.deltaTime);
        CountLifeTime();
    }

    public void OnTakePool()
    {
        startPos = transform.position;
        Debug.Log($"오브젝트 생성 : {startPos}");
    }

    public void ReturnToPool()
    {
        transform.position = startPos;
        _elapsed = 0;
        Debug.Log($"오브젝트 되돌림");
    }
    
    private void CountLifeTime()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= 5)
        {
            PoolManager.Instance.Return(PoolType.SecondPool, gameObject);
        }
    }
}
