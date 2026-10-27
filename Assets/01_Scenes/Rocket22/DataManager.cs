using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private MissionTable _missionTable;
    private SkinTable _skinTable;
    private StageTable _stageTable;

    public MissionTable GetMissionData()
    {
        return _missionTable;
    }
    
    public SkinTable GetSkinData()
    {
        return _skinTable;
    }
    
    public StageTable GetStageData()
    {
        return _stageTable;
    }

    private void Awake()
    {
        SetSingleton();
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
