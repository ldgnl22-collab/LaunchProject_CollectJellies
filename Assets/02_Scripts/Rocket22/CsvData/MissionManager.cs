using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    
    private TableType _type = TableType.Mission;
    
    // 미션 데이터 및 진행도 데이터 리스트
    private List<MissionData> _listMissionData = new List<MissionData>();
    private List<MissionProgress> _listMissionProgress =  new List<MissionProgress>();
    private List<MissionData> _listDailyMissionData = new List<MissionData>();
    
    public Action OnMissionUpdate;
    
    private void Awake() => SetSingleton();
    
    private void Start()
    {
        Init();
    }
    
    // 미션 텍스트 가져가기
    public string GetMissionDetail(int id)
    {
        string detail = string.Format(_listMissionData[id-1].Detail, _listMissionData[id-1].TargetValue);
        return detail;
    }

    // 모든 미션 데이터 가져가기
    public IEnumerable<MissionData> GetMissionDataList()
    {
        foreach (MissionData missionData in _listMissionData)
        {
            yield return missionData;
        }
    }

    // 랜덤 일일 미션 데이터 가져가기
    public IEnumerable<MissionData> GetDailyMissionList()
    {
        foreach (MissionData missionData in _listDailyMissionData)
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
                
                // 미션 업데이트 마다 저장
                GameManager.Instance.SaveMission(
                    i,
                    _listMissionProgress[i].MissionId,
                    _listMissionProgress[i].IsClear,
                    _listMissionProgress[i].CurProgress
                    );
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
    
    // 모든 미션 진행도 초기화
    public void ResetMissionProgress()
    {
        for (int i = 0; i < _listMissionProgress.Count; i++)
        {
            _listMissionProgress[i].ResetProgress();
        }
        
        // 랜덤 일일 미션 3개 초기화
        SetDailyMissionData(3);
    }

    // 모든 미션 진행도 불러오기
    private void LoadMissionProgress(int id, bool isClear, int progress)
    {
        for (int i = 0; i < _listMissionProgress.Count; i++)
        {
            _listMissionProgress[id].LoadProgress(isClear, progress);
        }
    }

    // 모든 미션 저장 (시작시 1회)
    public void SaveMissionProgress()
    {
        for (int i = 0; i < _listMissionProgress.Count; i++)
        {
            GameManager.Instance.SaveMission(
                i,
                _listMissionProgress[i].MissionId,
                _listMissionProgress[i].IsClear,
                _listMissionProgress[i].CurProgress
            );
        }
    }
    
    // 미션 n개 랜덤으로 주기
    public void SetDailyMissionData(int count)
    {
        if (count >= _listMissionData.Count)
        {
            count = _listMissionData.Count;
        }
        else if (count < 0)
        {
            count = 1;
        }
        
        int rand;
        
        List<int> randomMissionId = new();
        
        while (randomMissionId.Count < count)
        {
            rand = Random.Range(0, _listMissionData.Count);

            if (randomMissionId.Contains(_listMissionData[rand].Id))
            {
                // 같은 미션ID가 들어있으면
                continue;
            }

            randomMissionId.Add(_listMissionData[rand].Id);
        }
        
        randomMissionId.Sort();
        
        for (int i = 0; i < count; i++)
        {
            // Id 값을 넣었기 때문에 -1로 인덱스로 변경
            _listDailyMissionData.Add(_listMissionData[randomMissionId[i]-1]);
        }
    }
    
    private void Init()
    {
        InitMissionData();
        InitProgressData();
    }
    
    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
