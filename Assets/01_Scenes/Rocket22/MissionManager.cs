using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    private TableType _type = TableType.Mission;
    private List<MissionType> _listMissionType;
    
    // 미션 리스트
    private List<MissionData> _listMissionData = new List<MissionData>();
    
    private void Start()
    {
        Init();
    }

    public void UpdateMissionProgress(MissionType missionType)
    {
        foreach (MissionData missionData in _listMissionData)
        {
            if (missionData.IsClear || missionType != missionData.MissionType)
            {
                continue;
            }
            
            if (missionData.MissionType == missionType)
            {
                missionData.AddProgress(1);
            }
        }
    }

    public MissionData GetMissionData(int index)
    {
        return _listMissionData[index];
    }

    // public void SaveMissionData()
    // {
    //     for (int i = 0; i < _listMissionData.Count; i++)
    //     {
    //         GameManager.Instance.SaveMission(i,
    //             _listMissionData[i].Id,
    //             _listMissionData[i].IsClear,
    //             _listMissionData[i].CurProgress);
    //     }
    // }

    private void InitMissionType()
    {
        for (int i = 0; i < DataManager.Instance.GetDataCount(_type); i++)
        {
            Enum.TryParse(DataManager.Instance.GetData<string>(_type, i, "MissionType"), out MissionType missionType);
            _listMissionData[i].SetMissionType(missionType);
            
            Debug.Log(_listMissionData[i].MissionType);
        }
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
        InitMissionType();
    }
}
