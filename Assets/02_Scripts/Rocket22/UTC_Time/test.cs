using UnityEngine;

public class test : MonoBehaviour
{
    private void Start()
    {
        TimeManager.Instance.OnTimeAlone += PrintTime;
        TimeManager.Instance.OnTimeReset += PrintTimeReset;
    }

    private void PrintTime()
    {
        Debug.Log("1초 경과");
    }

    private void PrintTimeReset()
    {
        Debug.Log("초기화됨");
    }
}
