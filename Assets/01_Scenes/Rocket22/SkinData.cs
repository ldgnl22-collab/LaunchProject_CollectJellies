using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinData : MonoBehaviour
{
    private int _id;
    private string _name;

    public int Id => _id;
    public string Name => _name;

    public SkinData(int id, string name)
    {
        _id = id;
        _name = name;
    }
}
