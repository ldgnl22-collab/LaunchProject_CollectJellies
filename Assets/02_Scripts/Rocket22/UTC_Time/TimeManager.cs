using System;
using System.Collections;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private const int KOREA_STANDARD_TIME = 9;
    
    private DateTime _utcTime;
    private DateTime _nextResetTime;
    private WaitForSeconds _wait;

    public Action OnTimeReset;
    public Action OnTimeAlone;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        // 게임 종료 시간 불러오기
        
        // 접속 시간 확인
        DateTime nowTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);
        
        // 시간 비교
        // 종료 11.04 11:30, 시작 11.05 03:20
        // 종료했던 날 < 접속한 날(yy.mm.dd) -> 미션초기화
        // 종료 11.04 11:30, 시작 11.04 11:40
        // 종료했던 날 >= 접속한 날(yy.mm.dd) -> 아무일 없음
    }

    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    private void OnDisable()
    {
        // 게임 종료 시간 저장
    }
    
    private void ResetTime()
    {
        // 현재 UTC 시간 + KST_TIME = 한국 시간
        _utcTime = DateTime.UtcNow.AddHours(KOREA_STANDARD_TIME);
        
        // _utcTime 에 0시, 0분, 0초. 예) 26.11.04 00:00:00
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
                OnTimeReset?.Invoke();
                ResetTime();
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
