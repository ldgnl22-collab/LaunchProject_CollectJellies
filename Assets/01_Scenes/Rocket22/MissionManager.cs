using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    private TableType _type = TableType.Mission;
    
    // 미션 리스트
    private List<MissionData> _listMissionData = new List<MissionData>();
    
    // 미션 ID
    private int _missionID;
    
    // 진행 상황
    private int _curProgress;
    
    // 목표 수치
    private int _targetValue;
    
    // 미션 클리어 여부
    private bool _isClear;

    private void Start()
    {
        Init();
    }

    public void UpdateMissionProgress(MissionData missionData)
    {
        if (!missionData.IsClear)
        {
            missionData.AddProgress(1);
        }
    }

    public MissionData GetMissionData(int index)
    {
        return _listMissionData[index];
    }

    private void InitMissionData()
    {
        for (int i = 0; i < DataManager.Instance.GetDataCount(_type); i++)
        {
            MissionData missionData = new (
                DataManager.Instance.GetData<int>(_type, i, "MissionID"),
                DataManager.Instance.GetData<string>(_type, i, "Detail"),
                DataManager.Instance.GetData<int>(_type, i, "TargetValue"),
                DataManager.Instance.GetData<int>(_type, i, "Reward"));
            
            _listMissionData.Add(missionData);
        }
    }

    public void ResetMissionProgress()
    {
        for (int i = 0; i < _listMissionData.Count; i++)
        {
            _listMissionData[i].ResetProgress();
        }
    }
    
    private void Init()
    {
        InitMissionData();
    }
}
