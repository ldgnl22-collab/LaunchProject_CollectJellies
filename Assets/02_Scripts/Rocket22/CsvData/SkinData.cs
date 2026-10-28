using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkinData : IDataTable
{
    [field: SerializeField] public TextAsset CsvFile { get; private set; }

    private const int SKIN_DATA_COUNT = 2;
    
    private int _id;
    private string _name;

    public int CulomnCount { get; } = SKIN_DATA_COUNT;
    public int ID { get; }
    
    public int Id => _id;
    public string Name => _name;
    
    public SkinData(){}
    
    public SkinData(int id, string name)
    {
        ID = _id = id;
        _name = name;
    }
    
    public void SetData(string[] cells)
    {
        SkinData data;
        if (int.TryParse(cells[0], out int id))
        {
            data = new SkinData(id, cells[1]);
        }
    }
}
