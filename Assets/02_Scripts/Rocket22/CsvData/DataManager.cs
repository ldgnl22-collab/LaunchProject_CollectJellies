using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public enum TableType
{
    None = -1,
    Mission,
    Stage,
    Skin,
    
    Max
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    
    [SerializeField] private TextAsset _missionTable;
    [SerializeField] private TextAsset _stageTable;
    [SerializeField] private TextAsset _skinTable;

    private List<Dictionary<string, object>> _missionData;
    private List<Dictionary<string, object>> _stageData;
    private List<Dictionary<string, object>> _skinData;

    private void Awake()
    {
        SetSingleton();
        Init();
    }

    public void LoadMission()
    {
        for (int i = 0; i < _missionData.Count; i++)
        {
            Debug.Log(_missionData[i]["Detail"]);
        }
    }

    // public T GetData<T>(TableType type, int index, string key)
    // {
    //     List<Dictionary<string, object>> list = GetTypeList(type);
    //     
    //     Dictionary<string, object> dict = list[index];
    //     
    //     if (list[index][key].GetType() == typeof(int))
    //     {
    //         return 
    //     }
    //     else if (list[index][key].GetType() == typeof(float))
    //     {
    //         
    //     }
    // }

    private List<Dictionary<string, object>> GetTypeList(TableType type)
    {
        switch (type)
        {
            case TableType.Mission:
                return _missionData;
                break;
            case TableType.Stage:
                return _stageData;
                break;
            case TableType.Skin:
                return _skinData;
                break;
            default:
                return null;
                break;
        }
    }
    
    private void Init()
    {
        _missionData = CSVReader.ReadCSV(_missionTable);
        _skinData = CSVReader.ReadCSV(_skinTable);
        _stageData = CSVReader.ReadCSV(_stageTable);
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
