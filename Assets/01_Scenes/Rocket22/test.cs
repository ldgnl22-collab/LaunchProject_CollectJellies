using UnityEngine;

public class test : MonoBehaviour
{
    private int count = 1;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DataManager.Instance.MissionData.TryGetData(count, out MissionData data))
            {
                Debug.Log(data);
                count++;
            }
        }
    }
}
