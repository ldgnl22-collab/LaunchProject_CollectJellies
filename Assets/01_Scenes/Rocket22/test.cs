using System;
using UnityEngine;

public class test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MissionManager.Instance.ResetMissionProgress();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MissionManager.Instance.UpdateMissionProgress(MissionManager.Instance.GetMissionData(3));
        }
    }
}
