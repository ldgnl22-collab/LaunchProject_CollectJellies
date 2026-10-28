using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class MissionData : MonoBehaviour, IDataTable
{
    [field: SerializeField] public TextAsset CsvFile { get; private set; }

    private const int MISSION_DATA_COUNT = 4;
    
    private int _id;
    private string _detail;
    private int _targetValue;
    private int _reward;

    public int CulomnCount { get; } = MISSION_DATA_COUNT;
    public int ID { get; }
    
    public int Id => _id;
    public string Detail => _detail;
    public int TargetValue => _targetValue;
    public int Reward => _reward;
    
    public MissionData(int id, string detail, int targetValue, int reward)
    {
        ID = _id = id;
        _detail = detail;
        _targetValue = targetValue;
        _reward = reward;
    }

    public bool SetData(string[] cells)
    {
        if (int.TryParse(cells[0], out int id)
            && int.TryParse(cells[2], out int targetValue)
            && int.TryParse(cells[3], out int reward))
        {
            string result = string.Format(cells[1], targetValue);
            MissionData data = new MissionData(id, result, targetValue, reward);
            return true;
        }
        else
        {
            return false;
        }
    }
}
