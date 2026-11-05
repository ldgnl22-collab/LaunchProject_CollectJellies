using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    private TableType _type = TableType.Mission;
    
    // 미션 데이터 및 진행도 데이터 리스트
    private List<MissionData> _listMissionData = new List<MissionData>();
    private List<MissionProgress> _listMissionProgress =  new List<MissionProgress>();

    public Action OnMissionUpdate;
    
    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        BindOnMissionUpdate();
    }

    private void OnDisable()
    {
        UnBindOnMissionUpdate();
    }

    // 미션 텍스트 가져가기
    public string GetMissionDetail(int id)
    {
        string detail = string.Format(_listMissionData[id].Detail, _listMissionData[id].TargetValue);
        return detail;
    }

    // 미션 데이터 가져가기
    public IEnumerable<MissionData> GetMissionDataList()
    {
        foreach (MissionData missionData in _listMissionData)
        {
            yield return missionData;
        }
    }

    // 미션 진행정보 가져가기
    public IEnumerable<MissionProgress> GetMissionProgress()
    {
        foreach (MissionProgress missionProgress in _listMissionProgress)
        {
            yield return missionProgress;
        }
    }

    // 미션 진행도 추가 ('count'는 1 혹은 누적된 점수 값)
    public void UpdateMissionProgress(MissionType missionType, int count)
    {
        for (int i = 0; i < _listMissionProgress.Count; i++)
        {
            if (_listMissionProgress[i].IsClear||
                missionType != _listMissionData[i].MissionType)
            {
                continue;
            }

            if (missionType == _listMissionData[i].MissionType)
            {
                _listMissionProgress[i].AddProgress(_listMissionData[i], count);
                OnMissionUpdate?.Invoke();
                return;
            }
        }
    }
    
    private void InitMissionData()
    {
        for (int i = 0; i < DataManager.Instance.GetDataCount(_type); i++)
        {
            Enum.TryParse(DataManager.Instance.GetData<string>(_type, i, "MissionType"), out MissionType missionType);
            
            MissionData missionData = new (
                DataManager.Instance.GetData<int>(_type, i, "MissionID"),
                missionType,
                DataManager.Instance.GetData<string>(_type, i, "Detail"),
                DataManager.Instance.GetData<int>(_type, i, "TargetValue"),
                DataManager.Instance.GetData<int>(_type, i, "Reward"));
            
            _listMissionData.Add(missionData);
        }
    }

    private void InitProgressData()
    {
        for (int i = 0; i < DataManager.Instance.GetDataCount(_type); i++)
        {
            MissionProgress missionProgress = new MissionProgress(_listMissionData[i].Id);
            _listMissionProgress.Add(missionProgress);
        }
    }
    
    public void ResetMissionProgress()
    {
        for (int i = 0; i < _listMissionProgress.Count; i++)
        {
            _listMissionProgress[i].ResetProgress();
        }
    }
    
    private void Init()
    {
        InitMissionData();
        InitProgressData();
    }

    private void BindOnMissionUpdate()
    {
        TimeManager.Instance.OnTimeReset += ResetMissionProgress;
    }

    private void UnBindOnMissionUpdate()
    {
        TimeManager.Instance.OnTimeReset -= ResetMissionProgress;
    }
}
