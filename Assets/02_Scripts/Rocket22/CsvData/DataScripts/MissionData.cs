using System;
using UnityEngine;

[Serializable]
public class MissionData
{
    private int _id;
    private MissionType _missionType;
    private string _detail;
    private int _targetValue;
    private int _reward;
    
    public int Id => _id;
    public MissionType MissionType => _missionType;
    public string Detail => _detail;
    public int TargetValue => _targetValue;
    public int Reward => _reward;
    
    public MissionData(int id, MissionType missionType, string detail, int targetValue, int reward)
    {
        _id = id;
        _missionType = missionType;
        _detail = detail;
        _targetValue = targetValue;
        _reward = reward;
    }
}
