using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class MissionData
{
    private int _id;
    private string _detail;
    private int _targetValue;
    private int _reward;
    
    public int Id => _id;
    public string Detail => _detail;
    public int TargetValue => _targetValue;
    public int Reward => _reward;
    
    public MissionData(int id, string detail, int targetValue, int reward)
    {
        _id = id;
        _detail = detail;
        _targetValue = targetValue;
        _reward = reward;
    }
}
