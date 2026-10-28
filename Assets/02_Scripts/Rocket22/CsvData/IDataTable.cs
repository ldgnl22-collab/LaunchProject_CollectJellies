using System.Data;
using UnityEngine;

public interface IDataTable
{
    public TextAsset CsvFile { get; }
    
    public int CulomnCount { get; }
    
    public int ID { get; }
    
    public bool SetData(string[] cells);
}
