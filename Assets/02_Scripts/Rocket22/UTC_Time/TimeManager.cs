using System;
using System.Collections;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private DateTime _nowTime;
    private DateTime _nextResetTime;
    private WaitForSeconds _wait;

    public Action OnTimeReset;
    public Action OnTimeAlone;

    private void Awake()
    {
        Init();
    }
    
    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    private void SetResetTime()
    {
        // 현재 UTC 기준. 예) 26.11.04 16:30:30
        _nowTime = DateTime.Now;
        
        // 현재 UTC 기준 날짜에서 0시, 0분, 0초. 예) 26.11.04 00:00:00
        DateTime todayTime = new DateTime(_nowTime.Year, _nowTime.Month, _nowTime.Day, 17, 38, 0);

        _nextResetTime = _nowTime < todayTime ? todayTime : todayTime.AddDays(1);

        // DateTimeKind.Utc
    }

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            DateTime nowTime = DateTime.Now;

            if (nowTime >= _nextResetTime)
            {
                OnTimeReset?.Invoke();
                SetResetTime();
            }
            OnTimeAlone?.Invoke();

            yield return _wait;
        }
    }

    private void Init()
    {
        _wait = new WaitForSeconds(1f);
    }
}
