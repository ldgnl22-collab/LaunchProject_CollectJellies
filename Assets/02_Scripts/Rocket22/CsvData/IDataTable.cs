using System.Data;
using UnityEngine;

public interface IDataTable
{
    public int CulomnCount { get; }
    
    public int ID { get; }
    
    public void SetData(string[] cells);
}
