using System;
using UnityEngine;

public class JellyCollector : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _checkDistance = 1.2f;
    private RotateAction _rotateAction;
    
    public event Action OnCollected;
    
        

    private void Awake() => _rotateAction = GetComponent<RotateAction>();
    
    
    private void Update()
    {
        CheckItem();
    }

    private void CheckItem()
    {
        Ray ray = new Ray(transform.position, _rotateAction.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _checkDistance, _layerMask))
        {
            Debug.Log("Item감지");
            Destroy(hit.transform.gameObject);
            OnCollected?.Invoke();
            // 감지되면 -> Spawner 
        }
    }
}