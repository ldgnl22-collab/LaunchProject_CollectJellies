using System;
using System.Collections.Generic;
using UnityEngine;

public class CsvParse<T> where T : MonoBehaviour, IDataTable
{
    private T _csvData;
    
    private Dictionary<int, T> _dicDataById = new Dictionary<int, T>();

    private void Awake()
    {
    }

    public bool TryGetData(int id, out T data)
    {
        return _dicDataById.TryGetValue(id, out data);
    }

    public void LoadTable(T data)
    {
        string[] lines = data.CsvFile.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 2; i < lines.Length; i++)
        {
            ParseLine(i, lines[i]);
        }
    }

    private void ParseLine(int index, string line)
    {
        string[] cells = line.Split(',');

        if (cells.Length != _csvData.CulomnCount)
        {
            Debug.Log($"MissionData: {index}번 줄은 칸이 {cells.Length}개라 건너뜁니다: {line}");
            return;
        }
        
        TrimCells(cells);
        
        if (_csvData.SetData(cells))
        {
            AddData(_csvData);
        }
    }

    private void TrimCells(string[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = cells[i].Trim();
        }
    }
    
    private void AddData(T data)
    {
        if (_dicDataById.ContainsKey(data.ID))
        {
            Debug.Log($"MissionData: Id {data.ID}이 이미 있어 줄을 건너뜁니다.");
            return;
        }

        _dicDataById.Add(data.ID, data);
    }
}
