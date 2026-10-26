using UnityEngine;

public class BulletTest : MonoBehaviour
{
    private float _elapsed;
    
    private void Update()
    {
        transform.Translate(Vector3.forward * 5 * Time.deltaTime);
        CountLifeTime();
    }

    public void ResetState(Vector3 startPos)
    {
        transform.position = startPos;
        _elapsed = 0;
    }
    
    private void CountLifeTime()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= 3)
        {
            PoolManager.Instance.Return(PoolType.BulletPool, gameObject);
        }
    }
}
