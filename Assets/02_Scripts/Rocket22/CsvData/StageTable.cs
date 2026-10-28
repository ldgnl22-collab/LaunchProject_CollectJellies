using System;
using System.Collections.Generic;
using UnityEngine;

public class StageTable : MonoBehaviour
{
    private const int STAGE_DATA_COUNT = 2;
    [SerializeField] private TextAsset _csvFile;
    
    private List<StageData> _dataList = new List<StageData>();
    private Dictionary<int, StageData> _dicDataById = new Dictionary<int, StageData>();

    private void Awake()
    {
        LoadTable();
    }

    public bool TryGetData(int id, out StageData data)
    {
        return _dicDataById.TryGetValue(id, out data);
    }

    public void LoadTable()
    {
        string[] lines = _csvFile.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 2; i < lines.Length; i++)
        {
            ParseLine(i, lines[i]);
        }
    }

    private void ParseLine(int index, string line)
    {
        string[] cells = line.Split(',');

        if (cells.Length != STAGE_DATA_COUNT)
        {
            Debug.Log($"StageData: {index}번 줄은 칸이 {cells.Length}개라 건너뜁니다: {line}");
            return;
        }
        
        TrimCells(cells);
        
        if (int.TryParse(cells[0], out int id))
        {
            StageData data = new StageData(id, cells[1]);
            AddData(data);
        }
    }

    private void TrimCells(string[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = cells[i].Trim();
        }
    }
    
    private void AddData(StageData data)
    {
        if (_dicDataById.ContainsKey(data.Id))
        {
            Debug.Log($"StageData: Id {data.Id}이 이미 있어 {data.Name} 줄을 건너뜁니다.");
            return;
        }

        _dataList.Add(data);
        _dicDataById.Add(data.Id, data);
    }
}
