using System;
using System.Collections.Generic;
using UnityEngine;

public static class CSVReader
{
    public static List<Dictionary<string, object>> ReadCSV(TextAsset csvTable)
    {
        var list = new List<Dictionary<string, object>>();
        
        string[] lines = csvTable.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string[] header = lines[0].Split(',');

        for (int i = 2; i < lines.Length; i++)
        {
            string[] cells = lines[i].Split(',');
            if (cells.Length == 0)
            {
                continue;
            }

            if (cells.Length != header.Length)
            {
                Debug.Log($"MissionData: {i}번 줄은 칸이 {cells.Length}개라 건너뜁니다: {i}");
                continue;
            }
            
            var entry = new Dictionary<string, object>();
            
            for (int j = 0; j < header.Length; j++)
            {
                string value = cells[j].Trim();

                if (int.TryParse(value, out int numValue))
                {
                    entry[header[j]] = numValue;
                }
                else if (float.TryParse(value, out float floatValue))
                {
                    entry[header[j]] = floatValue;
                }
                else
                {
                    entry[header[j]] = value;
                }
            }

            list.Add(entry);
        }
        return list;
    }
}