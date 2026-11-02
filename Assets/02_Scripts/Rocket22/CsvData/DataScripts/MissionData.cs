using System;
using UnityEngine;

[Serializable]
public class MissionData
{
    private int _id;
    private string _detail;
    private int _targetValue;
    private int _reward;

    private MissionType _missionType;
    private int _curProgress;
    private bool _isClear;
    private bool _hasRewarded;
    
    public int Id => _id;
    public string Detail => _detail;
    public int TargetValue => _targetValue;
    public int Reward => _reward;
    
    public MissionType MissionType => _missionType;
    public int CurProgress => _curProgress;
    public bool IsClear => _isClear;
    public bool HasRewarded => _hasRewarded;
    
    public MissionData(int id, string detail, int targetValue, int reward)
    {
        _id = id;
        _detail = detail;
        _targetValue = targetValue;
        _reward = reward;
    }

    public void SetMissionType(MissionType missionType)
    {
        _missionType = missionType;
    }
    
    public void AddProgress(int curProgress)
    {
        if (_isClear)
        {
            return;
        }
        
        _curProgress += curProgress;
        if (_curProgress >= _targetValue)
        {
            Debug.Log($"{string.Format(_detail, _targetValue)} Cleared!");
            _isClear = true;
        }
    }

    public void ResetProgress()
    {
        _curProgress = 0;
        _isClear = false;
    }
}
