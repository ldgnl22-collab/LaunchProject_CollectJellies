using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet = PoolManager.Instance.Take(PoolType.BulletPool);

            if (bullet != null)
            {
                bullet.GetComponent<BulletTest>().ResetState(Vector3.zero);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameObject bomb = PoolManager.Instance.Take(PoolType.SecondPool);
        }
    }
}
