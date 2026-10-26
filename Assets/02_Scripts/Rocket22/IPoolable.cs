using UnityEngine;

public interface IPoolable
{
    public void OnTakePool();
    public void ReturnToPool();
}
