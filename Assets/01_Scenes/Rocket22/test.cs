using System;
using UnityEngine;

public class test : MonoBehaviour
{
    private int id;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MissionManager.Instance.ResetMissionProgress();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MissionManager.Instance.UpdateMissionProgress(MissionType.StageClearCount, 1);
        }
    }
}
