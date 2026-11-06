using System;
using UnityEngine;

public class MissionProgress
{
    private int _missionId;
    private int _curProgress;
    private bool _isClear;

    public int MissionId => _missionId;
    public int CurProgress => _curProgress;
    public bool IsClear => _isClear;
    
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

    public void LoadProgress(bool isClear, int progress)
    {
        _isClear = isClear;
        _curProgress = progress;
    }

    public void ResetProgress()
    {
        _curProgress = 0;
        _isClear = false;
    }
}
