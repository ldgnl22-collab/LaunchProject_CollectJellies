using UnityEngine;

public class Player : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet = PoolManager.Instance.Take();

            if (bullet != null)
            {
                bullet.GetComponent<Bullet>().ResetState(Vector3.zero);
            }
        }
    }
}
