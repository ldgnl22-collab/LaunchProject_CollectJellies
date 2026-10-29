using UnityEngine;

public class test : MonoBehaviour
{
    TableType _tableType;

    private void Start()
    {
        _tableType = TableType.Skin;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DataManager.Instance.LoadMission();
        }
    }
}
