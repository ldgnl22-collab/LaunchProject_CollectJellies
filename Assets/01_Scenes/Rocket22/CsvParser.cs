using System;
using System.Collections.Generic;
using UnityEngine;

public class CsvParser<T> where T : IDataTable, new()
{
    private TextAsset _csvFile;
    
    private Dictionary<int, T> _dicDataById = new Dictionary<int, T>();

    private void Awake()
    {
        LoadTable();
    }

    public bool TryGetData(int id, out T data)
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

        if (cells.Length != MISSION_DATA_COUNT)
        {
            Debug.Log($"MissionData: {index}번 줄은 칸이 {cells.Length}개라 건너뜁니다: {line}");
            return;
        }
        
        TrimCells(cells);
        
        if (int.TryParse(cells[0], out int id)
            && int.TryParse(cells[2], out int targetValue)
            && int.TryParse(cells[3], out int reward))
        {
            string result = string.Format(cells[1], targetValue);
            
            MissionData data = new MissionData(id, result, targetValue, reward);
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
    
    private void AddData(IDataTable data)
    {
        if (_dicDataById.ContainsKey(data.Id))
        {
            Debug.Log($"MissionData: Id {data.Id}이 이미 있어 {data.Detail} 줄을 건너뜁니다.");
            return;
        }

        _dicDataById.Add(data.Id, data);
    }
}
