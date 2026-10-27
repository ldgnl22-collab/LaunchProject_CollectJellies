using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<TextAsset> _csvFileList;

    private Dictionary<int, string[]> _splitCsv;

    private void Start() => CheckFirst();
    
    private void CheckFirst()
    {
        for (int i = 0; i < _csvFileList.Count; i++)
        {
            string[] lines = _csvFileList[i].text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] cells = lines[2].Split(',');
            string name = cells[1];

            Debug.Log($"Table: 줄 {lines.Length}개");
            Debug.Log($"Table: 이름 {name.Length}글자");
        }
    }
}
