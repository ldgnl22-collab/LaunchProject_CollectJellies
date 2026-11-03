using System;
using UnityEngine;

public class MissionProgress
{
    private int _missionId;
    private int _curProgress;
    private bool _isClear;
    private bool _hasRewarded;

    public int MissionId => _missionId;
    public int CurProgress => _curProgress;
    public bool IsClear => _isClear;
    public bool HasRewarded => _hasRewarded;
    
    public void AddProgress(MissionData missionData, int progress)
    {
        if (_isClear)
        {
            return;
        }
        
        _curProgress += progress;
        if (_curProgress >= missionData.TargetValue)
        {
            _isClear = true;
        }
    }

    public MissionProgress(int id)
    {
        _missionId = id;
    }

    public void ResetProgress()
    {
        _curProgress = 0;
        _isClear = false;
        _hasRewarded = false;
    }
}
