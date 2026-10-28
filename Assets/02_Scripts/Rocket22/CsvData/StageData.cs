using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData : IDataTable
{
    private const int STAGE_DATA_COUNT = 2;
    
    private int _id;
    private string _name;

    public int CulomnCount { get; } = STAGE_DATA_COUNT;
    public int ID { get; }
    
    public int Id => _id;
    public string Name => _name;

    public StageData(){}
    
    public StageData(int id, string name)
    {
        ID = _id = id;
        _name = name;
    }
    
    public void SetData(string[] cells)
    {
        if (int.TryParse(cells[0], out int id))
        {
            StageData data = new StageData(id, cells[1]);
        }
    }
}
