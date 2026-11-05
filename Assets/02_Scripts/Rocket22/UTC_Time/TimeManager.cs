using System;
using System.Collections;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private const int KOREA_STANDARD_TIME = 9;
    private const float ALONE_TIME = 1f;
    private const string SAVE_DATE_KEY = "Date";
    private const string DATE_FORMAT = "yy-MM-dd HH:mm:ss";
    
    private DateTime _utcTime;
    private DateTime _nextResetTime;
    
    private string _savedTime;
    private DateTime _lastPlayTime;
    
    private WaitForSeconds _wait;

    public Action OnTimeReset;
    public Action OnTimeAlone;

    private void Awake()
    {
        Init();
        LoadLastPlayTime();
    }

    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    private void OnDisable()
    {
        SaveLastPlayTime();
    }

    private void SaveLastPlayTime()
    {
        // 게임 종료 시간 저장
        DateTime nowTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);
        string saveTime = nowTime.ToString(DATE_FORMAT);
        
        PlayerPrefs.SetString(SAVE_DATE_KEY, saveTime);
    }
    
    private void LoadLastPlayTime()
    {
        // 게임 종료 시간 불러오기
        _savedTime = PlayerPrefs.GetString(SAVE_DATE_KEY);
        _lastPlayTime = DateTime.ParseExact(_savedTime, DATE_FORMAT, null);
        
        // 접속 시간 확인
        DateTime nowTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);
        
        // 종료했던 날 < 접속한 날(yy.mm.dd) -> 미션초기화
        if (_lastPlayTime.Date < nowTime.Date)
        {
            // 00시에 실행 이벤트
            OnTimeReset?.Invoke();
            ResetTime();
        }
    }
    
    private void ResetTime()
    {
        // 현재 UTC 시간 + KST_TIME = 한국 시간
        _utcTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);
        
        // _utcTime 에 0시 0분 0초. 예) 26.11.04 00:00:00
        DateTime todayTime = new DateTime(
            _utcTime.Year, _utcTime.Month, _utcTime.Day, 0, 0, 0
            );

        // 현재 시간이 같은날 00시 이전이면 00시 반환, 00시를 넘어가서 다음날이면 1일 후 00시 반환
        _nextResetTime = _utcTime < todayTime ? todayTime : todayTime.AddDays(1);
    }

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            DateTime nowTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);

            if (nowTime >= _nextResetTime)
            {
                // 00시에 실행 이벤트
                OnTimeReset?.Invoke();
                ResetTime();
            }
            // 1초씩 흐를 때 실행 이벤트
            OnTimeAlone?.Invoke();

            yield return _wait;
        }
    }

    private void Init()
    {
        _wait = new WaitForSeconds(ALONE_TIME);
    }
}
