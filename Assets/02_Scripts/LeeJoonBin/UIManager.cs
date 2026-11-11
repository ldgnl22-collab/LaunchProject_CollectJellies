using UnityEngine;

/// <summary> 전체 UI 시스템 및 상태를 관리하는 싱글톤 매니저 </summary>
public class UIManager : Singleton<UIManager>
{
    private const string CONTROL_INIT_KEY = "IsControlInitialized";
    private const string CONTROL_TYPE_KEY = "SelectedControlType";
    private const string SELECTED_SKIN_KEY = "SelectedSkinIndex";

    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private PopupManager _popupManager;

    private EControlType _eCurControlType;
    private EGameModeType _eCurGameModeType = EGameModeType.Normal;
    private EDifficultyType _eCurDifficultyType;
    private int _selectedStageIndex;
    private int _selectedSkinIndex;

    public WindowManager WindowManager => _windowManager;
    public PopupManager PopupManager => _popupManager;
    public EControlType ECurControlType => _eCurControlType;
    public EGameModeType ECurGameModeType => _eCurGameModeType;
    public EDifficultyType ECurDifficultyType => _eCurDifficultyType;
    public int SelectedStageIndex => _selectedStageIndex;
    public int SelectedSkinIndex => _selectedSkinIndex;

    private void Awake()
    {
        SetSingleton();
        LoadLocalUISettings();
    }

    private void OnEnable()
    {
        BindLoadingEvents();
    }

    private void OnDisable()
    {
        UnbindLoadingEvents();
    }

    /// <summary> 씬별 WindowManager 및 PopupManager 참조 갱신 </summary>
    public void RegisterSceneManagers(WindowManager windowManager, PopupManager popupManager)
    {
        _windowManager = windowManager;
        _popupManager = popupManager;
    }

    /// <summary> 최초 실행 시 조작법 선택 완료 여부 확인 </summary>
    public bool IsControlInitialized()
    {
        return PlayerPrefs.GetInt(CONTROL_INIT_KEY, 0) == 1;
    }

    /// <summary> 조작법 선택 저장 </summary>
    public void SaveControlType(EControlType controlType)
    {
        _eCurControlType = controlType;
        PlayerPrefs.SetInt(CONTROL_TYPE_KEY, (int)controlType);
        PlayerPrefs.SetInt(CONTROL_INIT_KEY, 1);
        PlayerPrefs.Save();
    }

    /// <summary> 스킨 장착 인덱스 저장 </summary>
    public void SaveSelectedSkin(int skinIndex)
    {
        _selectedSkinIndex = skinIndex;
        PlayerPrefs.SetInt(SELECTED_SKIN_KEY, skinIndex);
        PlayerPrefs.Save();
    }

    /// <summary> 게임 모드 설정 </summary>
    public void SelectGameMode(EGameModeType gameModeType)
    {
        _eCurGameModeType = gameModeType;
    }

    /// <summary> 선택한 스테이지 및 게임 모드 설정 </summary>
    public void SelectStageAndGameMode(int stageIndex, EGameModeType gameModeType)
    {
        _selectedStageIndex = stageIndex;
        _eCurGameModeType = gameModeType;
    }

    /// <summary> 선택한 스테이지 및 난이도 설정 (호환용) </summary>
    public void SelectStageAndDifficulty(int stageIndex, EDifficultyType difficultyType)
    {
        _selectedStageIndex = stageIndex;
        _eCurDifficultyType = difficultyType;
    }

    private void LoadLocalUISettings()
    {
        _eCurControlType = (EControlType)PlayerPrefs.GetInt(CONTROL_TYPE_KEY, (int)EControlType.TouchThreeSplit);
        _selectedSkinIndex = PlayerPrefs.GetInt(SELECTED_SKIN_KEY, 0);
    }

    private void BindLoadingEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnProgressChanged += HandleProgressChanged;
        GameManager.Instance.OnLoadingComplete += HandleLoadingComplete;
    }

    private void UnbindLoadingEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnProgressChanged -= HandleProgressChanged;
        GameManager.Instance.OnLoadingComplete -= HandleLoadingComplete;
    }

    private void HandleProgressChanged(float progress)
    {
        if (_windowManager == null)
        {
            return;
        }

        _windowManager.ShowLoading();
        _windowManager.SetLoadingProgress(progress);
    }

    private void HandleLoadingComplete()
    {
        if (_windowManager == null)
        {
            return;
        }

        _windowManager.HideLoading();
    }
}
