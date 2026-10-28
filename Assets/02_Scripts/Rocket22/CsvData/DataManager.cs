using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private CsvParse<MissionData> _missionData;
    private CsvParse<SkinData> _skinData;
    private CsvParse<StageData> _stageData;
    
    [SerializeField] private TextAsset _missionTable;
    [SerializeField] private TextAsset _skinTable;
    [SerializeField] private TextAsset _stageTable;

    public CsvParse<MissionData> MissionData => _missionData;
    public CsvParse<SkinData> SkinData => _skinData;
    public CsvParse<StageData> StageData => _stageData;
    
    private void Awake()
    {
        SetSingleton();
        Init();
    }

    private void Init()
    {
        _missionData.SetDataTable(new MissionData(), _missionTable);
        _skinData.SetDataTable(new SkinData(), _skinTable);
        _stageData.SetDataTable(new StageData(), _stageTable);
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
