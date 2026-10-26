using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    private void Start()
    {
    }
    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PoolManager.Instance.Take(PoolType.BulletPool);
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            PoolManager.Instance.Take(PoolType.SecondPool);
        }
    }
}
