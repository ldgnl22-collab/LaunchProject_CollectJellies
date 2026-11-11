using GravityCollect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> 인게임 씬(02_Game) HUD, 결과창, 환경설정 팝업 UI 컨트롤러 </summary>
public class GameSceneUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private PopupManager _popupManager;

    [Header("1. InGame HUD UI (All Image)")]
    [SerializeField] private Image _scoreBG;
    [SerializeField] private Image _scoreStarImg;
    [SerializeField] private Image _scoreValueImg;
    [SerializeField] private Image _stageInfoImg;
    [SerializeField] private Image _pauseBtnImg;
    [SerializeField] private Button _pauseBtn;

    [Header("2. Control Type UI Groups")]
    [SerializeField] private GameObject _controlType1GroupObj;
    [SerializeField] private Button _touchLeftBtn;
    [SerializeField] private Button _touchCenterBtn;
    [SerializeField] private Button _touchRightBtn;

    [SerializeField] private GameObject _controlType2GroupObj;
    [SerializeField] private Image _leftMoveBtnImg;
    [SerializeField] private Button _leftMoveBtn;
    [SerializeField] private Image _jumpBtnImg;
    [SerializeField] private Button _jumpBtn;
    [SerializeField] private Image _rightMoveBtnImg;
    [SerializeField] private Button _rightMoveBtn;

    [SerializeField] private GameObject _controlType3GroupObj;

    [Header("3. Clear Result Popup UI (All Image)")]
    [SerializeField] private Image _clearResultBG;
    [SerializeField] private Image[] _arrClearStarImg;
    [SerializeField] private Image _clearScoreBG;
    [SerializeField] private Image _clearScoreImg;
    [SerializeField] private Image _clearJellyBG;
    [SerializeField] private Image _clearJellyIconImg;
    [SerializeField] private Image _clearJellyCntImg;
    [SerializeField] private Image _clearRetryBG;
    [SerializeField] private Button _clearRetryBtn;
    [SerializeField] private Image _clearRetryLabelImg;
    [SerializeField] private Image _clearNextBG;
    [SerializeField] private Button _clearNextBtn;
    [SerializeField] private Image _clearNextLabelImg;
    [SerializeField] private Image _clearMainBG;
    [SerializeField] private Button _clearMainBtn;
    [SerializeField] private Image _clearMainLabelImg;

    [Header("4. Fail Result Popup UI (All Image)")]
    [SerializeField] private Image _failResultBG;
    [SerializeField] private Image _failTitleImg;
    [SerializeField] private Image _failScoreBG;
    [SerializeField] private Image _failScoreImg;
    [SerializeField] private Image _failJellyBG;
    [SerializeField] private Image _failJellyIconImg;
    [SerializeField] private Image _failJellyCntImg;
    [SerializeField] private Image _failRetryBG;
    [SerializeField] private Button _failRetryBtn;
    [SerializeField] private Image _failRetryLabelImg;
    [SerializeField] private Image _failMainBG;
    [SerializeField] private Button _failMainBtn;
    [SerializeField] private Image _failMainLabelImg;

    [Header("5. InGame Setting Popup UI (800x820, All Image)")]
    [SerializeField] private Image _settingPopupBG;
    [SerializeField] private Image _settingTitleImg;
    [SerializeField] private Image _settingCloseBtnImg;
    [SerializeField] private Button _settingCloseBtn;
    [SerializeField] private Image _bgmIconImg;
    [SerializeField] private Button _bgmMuteBtn;
    [SerializeField] private Image _bgmGaugeBG;
    [SerializeField] private Image _bgmGaugeImg;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Image _bgmHandleImg;
    [SerializeField] private Image _sfxIconImg;
    [SerializeField] private Button _sfxMuteBtn;
    [SerializeField] private Image _sfxGaugeBG;
    [SerializeField] private Image _sfxGaugeImg;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Image _sfxHandleImg;
    [SerializeField] private Image _resumeBtnBG;
    [SerializeField] private Button _resumeBtn;
    [SerializeField] private Image _resumeLabelImg;
    [SerializeField] private Image _mainBtnBG;
    [SerializeField] private Button _mainBtn;
    [SerializeField] private Image _mainLabelImg;

    [Header("Player Control Settings")]
    [SerializeField] private float _rotationSpeed = 1000f;

    private Transform _playerTransform;
    private JumpActionSample _jumpActionSample;
    private RotateAction _rotateAction;
    private JellyCollector _jellyCollector;
    private SlimeMotion _slimeMotion;

    private Quaternion _targetRotation;
    private EControlType _eCurControlType = EControlType.BottomButton;
    private int _curScore = 0;
    private int _curJellyCount = 0;
    private bool _isGameEnded = false;

    public int CurScore => _curScore;
    public int CurJellyCount => _curJellyCount;

    private void Awake()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterSceneManagers(_windowManager, _popupManager);
        }

        BindButtons();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (_popupManager != null)
        {
            _popupManager.HideAllPopups();
        }

        if (_windowManager != null)
        {
            _windowManager.ShowWindow(EWindowType.InGame);
        }

        _eCurControlType = UIManager.Instance != null ? UIManager.Instance.ECurControlType : EControlType.BottomButton;
        ApplyControlTypeUI(_eCurControlType);

        TryBindPlayer();
    }

    private void Update()
    {
        if (!_isGameEnded && (_playerTransform == null || _slimeMotion == null))
        {
            TryBindPlayer();
        }

        // 플레이어 사망 상태 감지
        if (!_isGameEnded && _slimeMotion != null && _slimeMotion.IsDead)
        {
            TriggerFailResult();
            return;
        }

        // 조작법 1, 2 사용 시 UI 버튼 기반 회전 보간 처리
        if (!_isGameEnded && _playerTransform != null && _eCurControlType != EControlType.Swipe)
        {
            _playerTransform.rotation = Quaternion.RotateTowards(
                _playerTransform.rotation,
                _targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
    }

    private void OnDestroy()
    {
        UnbindPlayerEvents();
    }

    /// <summary> 씬 내의 플레이어 오브젝트를 검색하여 조작 및 수집 이벤트 바인딩 </summary>
    public void TryBindPlayer()
    {
        if (_playerTransform != null)
        {
            return;
        }

        RotateAction rotate = Object.FindFirstObjectByType<RotateAction>();
        if (rotate == null)
        {
            return;
        }

        _playerTransform = rotate.transform;
        _rotateAction = rotate;
        _jumpActionSample = rotate.GetComponent<JumpActionSample>();
        _jellyCollector = rotate.GetComponent<JellyCollector>();
        _slimeMotion = rotate.GetComponent<SlimeMotion>();

        _targetRotation = _playerTransform.rotation;

        if (_jellyCollector != null)
        {
            _jellyCollector.OnCollected += HandleJellyCollected;
            _jellyCollector.OnReturn += HandlePlayerTrapReturn;
        }

        ApplyControlTypeUI(_eCurControlType);
    }

    private void UnbindPlayerEvents()
    {
        if (_jellyCollector != null)
        {
            _jellyCollector.OnCollected -= HandleJellyCollected;
            _jellyCollector.OnReturn -= HandlePlayerTrapReturn;
        }
    }

    /// <summary> 현재 선택된 조작법에 따라 하단/터치 UI 활성화 및 입력 모드 설정 </summary>
    public void ApplyControlTypeUI(EControlType controlType)
    {
        _eCurControlType = controlType;

        if (_controlType1GroupObj != null) _controlType1GroupObj.SetActive(controlType == EControlType.TouchThreeSplit);
        if (_controlType2GroupObj != null) _controlType2GroupObj.SetActive(controlType == EControlType.BottomButton);
        if (_controlType3GroupObj != null) _controlType3GroupObj.SetActive(controlType == EControlType.Swipe);

        // 조작법 1, 2에서는 플레이어 본체의 스와이프 InputAction을 끄고 UI 터치로 회전 및 점프 수행
        if (_rotateAction != null)
        {
            _rotateAction.enabled = (controlType == EControlType.Swipe);
        }

        if (_playerTransform != null)
        {
            _targetRotation = _playerTransform.rotation;
        }
    }

    private void BindButtons()
    {
        // 1. 일시정지 및 설정 창 버튼
        if (_pauseBtn != null)
        {
            _pauseBtn.onClick.AddListener(() =>
            {
                // if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SFXType.Button);
                Time.timeScale = 0f;
                _popupManager.ShowPopup(EPopupType.Setting);
            });
        }

        if (_settingCloseBtn != null)
        {
            _settingCloseBtn.onClick.AddListener(ResumeGame);
        }

        if (_resumeBtn != null)
        {
            _resumeBtn.onClick.AddListener(ResumeGame);
        }

        if (_mainBtn != null)
        {
            _mainBtn.onClick.AddListener(GoToLobby);
        }

        // 2. 조작법 1 (화면 3분할 터치) 버튼 바인딩
        if (_touchLeftBtn != null)
        {
            _touchLeftBtn.onClick.AddListener(RotateLeft);
        }

        if (_touchCenterBtn != null)
        {
            _touchCenterBtn.onClick.AddListener(DoJump);
        }

        if (_touchRightBtn != null)
        {
            _touchRightBtn.onClick.AddListener(RotateRight);
        }

        // 3. 조작법 2 (하단 좌/우/점프 버튼) 바인딩
        if (_leftMoveBtn != null)
        {
            _leftMoveBtn.onClick.AddListener(RotateLeft);
        }

        if (_jumpBtn != null)
        {
            _jumpBtn.onClick.AddListener(DoJump);
        }

        if (_rightMoveBtn != null)
        {
            _rightMoveBtn.onClick.AddListener(RotateRight);
        }

        // 4. 결과창 버튼 바인딩
        if (_clearRetryBtn != null) _clearRetryBtn.onClick.AddListener(RestartStage);
        if (_clearNextBtn != null) _clearNextBtn.onClick.AddListener(NextStage);
        if (_clearMainBtn != null) _clearMainBtn.onClick.AddListener(GoToLobby);

        if (_failRetryBtn != null) _failRetryBtn.onClick.AddListener(RestartStage);
        if (_failMainBtn != null) _failMainBtn.onClick.AddListener(GoToLobby);

        // 5. 사운드 볼륨 제어 바인딩
        if (_bgmMuteBtn != null)
        {
            _bgmMuteBtn.onClick.AddListener(() =>
            {
                // if (SoundManager.Instance != null) SoundManager.Instance.MuteBGM();
            });
        }

        if (_sfxMuteBtn != null)
        {
            _sfxMuteBtn.onClick.AddListener(() =>
            {
                // if (SoundManager.Instance != null) SoundManager.Instance.MuteSFX();
            });
        }

        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.AddListener(val =>
            {
                if (_bgmGaugeImg != null) _bgmGaugeImg.fillAmount = val;
                // if (SoundManager.Instance != null) SoundManager.Instance.BGMVolume(val);
            });
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.AddListener(val =>
            {
                if (_sfxGaugeImg != null) _sfxGaugeImg.fillAmount = val;
                // if (SoundManager.Instance != null) SoundManager.Instance.SFXVolume(val);
            });
        }
    }

    /// <summary> 좌회전 (왼쪽으로 90도 회전) </summary>
    public void RotateLeft()
    {
        if (_isGameEnded || _playerTransform == null)
        {
            return;
        }

        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        _targetRotation *= Quaternion.Euler(0f, -90f, 0f);

        if (_rotateAction != null)
        {
            _rotateAction.SetTargetRotation(_targetRotation);
        }
    }

    /// <summary> 우회전 (오른쪽으로 90도 회전) </summary>
    public void RotateRight()
    {
        if (_isGameEnded || _playerTransform == null)
        {
            return;
        }

        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        _targetRotation *= Quaternion.Euler(0f, 90f, 0f);

        if (_rotateAction != null)
        {
            _rotateAction.SetTargetRotation(_targetRotation);
        }
    }

    /// <summary> 점프 실행 </summary>
    public void DoJump()
    {
        if (_isGameEnded || _jumpActionSample == null)
        {
            return;
        }

        _jumpActionSample.Jump();
    }

    /// <summary> 젤리 획득 시 HUD 및 미션 진행도 갱신 </summary>
    private void HandleJellyCollected()
    {
        if (_isGameEnded)
        {
            return;
        }

        _curJellyCount++;
        _curScore += 100;

        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.ItemDrop);
        }
        */

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.UpdateMissionProgress(MissionType.GetStageJellyCount, 1);
            MissionManager.Instance.UpdateMissionProgress(MissionType.GetItemCount, 1);
        }
    }

    /// <summary> 트랩 충돌 시 실패 처리 </summary>
    private void HandlePlayerTrapReturn()
    {
        TriggerFailResult();
    }

    /// <summary> 스테이지 클리어 결과창 활성화 </summary>
    /// <param name="starCount"> 획득한 별 개수 (1 ~ 3) </param>
    public void TriggerClearResult(int starCount)
    {
        if (_isGameEnded)
        {
            return;
        }

        _isGameEnded = true;
        Time.timeScale = 0f;

        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Stage_Clear);
        }
        */

        int stageIdx = UIManager.Instance != null ? UIManager.Instance.SelectedStageIndex : 0;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveStage(stageIdx, true, _curScore, starCount);
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.UpdateMissionProgress(MissionType.StageClearCount, 1);
            MissionManager.Instance.UpdateMissionProgress(MissionType.StageClearScore, _curScore);
        }

        if (_arrClearStarImg != null)
        {
            for (int i = 0; i < _arrClearStarImg.Length; i++)
            {
                if (_arrClearStarImg[i] != null)
                {
                    _arrClearStarImg[i].gameObject.SetActive(i < starCount);
                }
            }
        }

        if (_popupManager != null)
        {
            _popupManager.ShowPopup(EPopupType.ClearResult);
        }
    }

    /// <summary> 스테이지 실패 결과창 활성화 </summary>
    public void TriggerFailResult()
    {
        if (_isGameEnded)
        {
            return;
        }

        _isGameEnded = true;
        Time.timeScale = 0f;

        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Stage_Failed);
        }
        */

        if (_popupManager != null)
        {
            _popupManager.ShowPopup(EPopupType.FailResult);
        }
    }

    private void ResumeGame()
    {
        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        Time.timeScale = 1f;
        _popupManager.HidePopup(EPopupType.Setting);
    }

    private void RestartStage()
    {
        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void NextStage()
    {
        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        Time.timeScale = 1f;
        int nextStage = (UIManager.Instance != null ? UIManager.Instance.SelectedStageIndex : 0) + 1;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SelectStageAndDifficulty(nextStage, UIManager.Instance.ECurDifficultyType);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToLobby()
    {
        /*
        try
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Button);
            }
        }
        catch { }
        */

        Time.timeScale = 1f;
        SceneManager.LoadScene("01_Lobby");
    }
}

