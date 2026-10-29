using System.Collections.Generic;
using UnityEngine;

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
    
    public T GetData<T>(TableType type, int index, string key)
    {
        Dictionary<string, object> dict = GetTypeList(type)[index];

        return dict.ContainsKey(key) ? (T)dict[key] : default(T);
    }

    public int GetDataCount(TableType type)
    {
        return GetTypeList(type).Count;
    }

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
