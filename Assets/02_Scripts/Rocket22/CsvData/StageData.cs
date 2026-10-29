using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    private int _id;
    private string _name;
    
    public int Id => _id;
    public string Name => _name;

    public StageData(){}
    
    public StageData(int id, string name)
    {
        _id = id;
        _name = name;
    }
}
