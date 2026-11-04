using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private const string NICK_NAME_KEY = "PlayerNickName";
    [HideInInspector] public string UserNickName;

    // UI가 받을 게이지 현황
    public event Action<float> OnProgressChanged;
    // UI가 받을 로딩 완료
    public event Action OnLoadingComplete;
    // 로딩 할 리스트
    private List<Func<IEnumerator>> _listLoading = new List<Func<IEnumerator>>();

    // 현재 씬 타입
    private SceneType _curSceneType;

    private SaveLoadManager _saveLoadManager;

    private const string LastCheckTimeKey = "LastMissionCheckTime";

    // 다른 스크립트(Mission 등)에서 일일 미션 초기화 여부를 확인할 수 있는 프로퍼티
    public bool IsResetTriggered { get; private set; } = false;

    private void Awake()
    {
        SetSingleton();

        if (Instance == this)
        {
            _saveLoadManager = new SaveLoadManager();
            _saveLoadManager.LoadData();
        }

        // 게임 시작 시 일일 미션 초기화 체크
        CheckDailyReset();
    }

    private void Start()
    {
        _curSceneType = SceneType.Title;

        // 플레이어 닉네임 있는지 확인.
        if (PlayerPrefs.GetString(NICK_NAME_KEY) == null)
        {
            // 닉네임이 없다면 UI 에게 알려주자.
        }
        else // 닉네임이 있다면
        {
            UserNickName = PlayerPrefs.GetString(NICK_NAME_KEY);
        }
    }

    /// <summary> 씬 이름 </summary>
    /// <param name="sceneType"> 씬 타입 </param>
    /// <returns> 변경할 씬 이름</returns>
    private string SceneName(SceneType sceneType)
    {
        return sceneType.ToString();
    }

    /// <summary> 씬 전환 </summary>
    /// <param name="sceneType"></param>
    public void StartAsyncLoad(SceneType sceneType)
    {
        // 같은 씬 전환 막기
        if (_curSceneType == sceneType)
            return;

        // 로딩 작업 준비 (매번 새로 로드할 때를 위해 리스트 초기화 후 세팅)
        _listLoading.Clear();

        // 로딩 순서
        // 풀링
        _listLoading.Add(InitObjectPool);
        // 데이터
        _listLoading.Add(InitDataParser);
        // 저장 데이터
        _listLoading.Add(InitLoadData);

        // 씬 전환 시작
        StartCoroutine(LoadSequence(sceneType));
    }

    /// <summary> 비동기 씬 전환 </summary>
    /// <param name="sceneType"> 전환할 씬 </param>
    /// <returns></returns>
    private IEnumerator LoadSequence(SceneType sceneType)
    {
        // 로딩바 게이지 시작 0
        float currentProgress = 0f;
        // UI에 로딩바 게이지 전달
        OnProgressChanged?.Invoke(0f);

        // 비동기 로드 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneName(sceneType));
        // 화면 전환 false
        operation.allowSceneActivation = false;

        // 완료 게이지 90프로로 조절하기 위해
        float progressPer = (1 / (float)_listLoading.Count) * 0.9f;

        // 순서대로 로딩
        for (int i = 0; i < _listLoading.Count; i++)
        {
            // 완료 될때까지 대기
            yield return StartCoroutine(_listLoading[i]());

            // 로딩 바 가야할 게이지 위치
            float targetProgress = (i + 1) * progressPer;

            // 로딩 바 연출
            while (currentProgress < targetProgress)
            {
                currentProgress += Time.deltaTime * 0.5f;
                // UI 에게 현재 게이지 전달
                OnProgressChanged?.Invoke(Mathf.Min(currentProgress, targetProgress));
                yield return null;
            }
        }

        // 로딩이 전체 완료되면 게이지는 90% 도달

        // 유니티 자체가 완료되었는지 확인
        // 유니티 자체에서 0 ~ 90% 가 씬 전환이고 90 ~ 100% 는 씬을 활성화 하는 단계이기 때문
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // 마지막 100% 채우기 마무리 연출
        while (currentProgress < 1f)
        {
            // 현재 게이지 증가
            currentProgress += Time.deltaTime * 2f;
            // UI 에게 현재 게이지 전달
            OnProgressChanged?.Invoke(Mathf.Min(currentProgress, 1f));
            yield return null;
        }

        // 완료시 UI에게 전달
        OnLoadingComplete?.Invoke();
        // 완료 했으니 화면 전환 True
        operation.allowSceneActivation = true;
        // 현재 씬이 어딘지 저장
        _curSceneType = sceneType;
    }

    /// <summary> 로딩 : 오브젝트 풀링 </summary>
    /// <returns></returns>
    private IEnumerator InitObjectPool()
    {
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary> 로딩 : 데이터 파싱 </summary>
    /// <returns></returns>
    private IEnumerator InitDataParser()
    {
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary> 로딩 : 세이브 데이터 로드 </summary>
    /// <returns></returns>
    private IEnumerator InitLoadData()
    {
        LoadContinue();

        yield return new WaitForSeconds(0.3f);
    }

    /// <summary> UTC 0시 기준 날짜가 바뀌었는지 체크하는 함수 </summary>
    public void CheckDailyReset()
    {
        // 현재 UTC 시간 체크
        DateTime currentUtcTime = DateTime.UtcNow;

        // 전에 저장한 시간 체크
        if (PlayerPrefs.HasKey(LastCheckTimeKey))
        {
            // 있다면 전 시간 확인
            string savedTimeString = PlayerPrefs.GetString(LastCheckTimeKey);

            // 변환
            if (DateTime.TryParse(savedTimeString, out DateTime lastCheckTime))
            {
                // 마지막 저장된 날짜보다 현재 UTC 날짜가 더 크면 (날짜가 바뀌었다면)
                if (currentUtcTime.Date > lastCheckTime.Date)
                {
                    // 날짜 변경 확인 리셋
                    DailyReset(currentUtcTime);
                }
                else
                {
                    // 날짜 변경 안됬으면 유지
                    IsResetTriggered = false;
                }
            }
        }
        else
        {
            // 첫 접속인 경우 초기화 및 저장
            DailyReset(currentUtcTime);
        }
    }

    private void DailyReset(DateTime currentUtcTime)
    {
        // 날짜 바뀐거 알려주기.
        IsResetTriggered = true;

        // 날짜가 바뀌었으니 현재 바뀐 날짜 시간 기록
        SaveCurrentTime(currentUtcTime);
    }

    private void SaveCurrentTime(DateTime timeToSave)
    {
        string timeString = timeToSave.ToString("o");
        PlayerPrefs.SetString(LastCheckTimeKey, timeString);
        PlayerPrefs.Save();
    }

    /// <summary> 초기화 처리를 완료한 후 호출하여 트리거를 꺼주는 함수 </summary>
    public void CompleteResetNotify()
    {
        // 다른곳에서 초기화 완료되었다면 최종 false 하기
        IsResetTriggered = false;
    }


    public void SaveStage(int index, bool isClear, int score, int starCnt)
    {
        SaveCurrentTime(DateTime.UtcNow);
        _saveLoadManager.StageSaveData(index, isClear, score, starCnt);
    }

    public void SaveSkin(int index, bool isSkin)
    {
        SaveCurrentTime(DateTime.UtcNow);
        _saveLoadManager.SkinSaveData(index, isSkin);
    }

    public void SaveMission(int index, int dataIndex, bool isClear, int progress)
    {
        SaveCurrentTime(DateTime.UtcNow);
        _saveLoadManager.MissionSaveData(index, dataIndex, isClear, progress);
    }

    public void SaveForceQuit(int mapIndex, Transform playerTr, int jellies, int score, Transform[] itemTr, int[] itemType)
    {
        SaveCurrentTime(DateTime.UtcNow);
        _saveLoadManager.ForceQuitSaveData(mapIndex, playerTr, jellies, score, itemTr, itemType);
    }

    public void LoadContinue()
    {
        _saveLoadManager.ContinueLoadData();
    }
}