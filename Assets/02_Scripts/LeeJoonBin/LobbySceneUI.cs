using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> 로비 씬(01_Lobby) 전체 Window 및 PopUp UI 컨트롤러 </summary>
public class LobbySceneUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private PopupManager _popupManager;

    [Header("1. Control Select Window UI")]
    [SerializeField] private Image _controlSelectBG;
    [SerializeField] private Image _controlTitleImg;
    [SerializeField] private Image[] _arrControlBtnImg;
    [SerializeField] private Button[] _arrControlBtn;
    [SerializeField] private Image[] _arrControlLabelImg;
    [SerializeField] private Image _controlPreviewImg;
    [SerializeField] private Sprite[] _arrControlPreviewSprite;
    [SerializeField] private Image _confirmBtnBG;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Image _confirmLabelImg;

    [Header("2. Lobby Window UI (Gold uses Text)")]
    [SerializeField] private Image _lobbyBG;
    [SerializeField] private Image _goldBG;
    [SerializeField] private Image _goldIconImg;
    [SerializeField] private Text _goldTxt;
    [SerializeField] private Image _lobbySettingBtnImg;
    [SerializeField] private Button _lobbySettingBtn;
    [SerializeField] private Image _skinBtnImg;
    [SerializeField] private Button _skinBtn;
    [SerializeField] private Image _stageBtnImg;
    [SerializeField] private Button _stageBtn;
    [SerializeField] private Image _missionBtnImg;
    [SerializeField] private Button _missionBtn;

    [Header("3. Skin Window UI")]
    [SerializeField] private Image _skinBG;
    [SerializeField] private Image _skinTitleImg;
    [SerializeField] private Image _skinSettingBtnImg;
    [SerializeField] private Button _skinSettingBtn;
    [SerializeField] private Image _skinBackBtnImg;
    [SerializeField] private Button _skinBackBtn;
    [SerializeField] private ScrollRect _skinScroll;
    [SerializeField] private Image[] _arrSkinSlotBG;
    [SerializeField] private Button[] _arrSkinSlotBtn;
    [SerializeField] private Image[] _arrSkinLockImg;
    [SerializeField] private Image _skinPreviewBG;
    [SerializeField] private Image _skinPreviewImg;

    [Header("4. Stage Window UI")]
    [SerializeField] private Image _stageBG;
    [SerializeField] private Image _stageTitleImg;
    [SerializeField] private Image _stageSettingBtnImg;
    [SerializeField] private Button _stageSettingBtn;
    [SerializeField] private Image _stageBackBtnImg;
    [SerializeField] private Button _stageBackBtn;

    [Header("Mode Selection (Normal vs Infinite)")]
    [SerializeField] private Image _normalModeBtnImg;
    [SerializeField] private Button _normalModeBtn;
    [SerializeField] private Image _normalModeLabelImg;
    [SerializeField] private Image _infiniteModeBtnImg;
    [SerializeField] private Button _infiniteModeBtn;
    [SerializeField] private Image _infiniteModeLabelImg;

    [SerializeField] private ScrollRect _stageScroll;
    [SerializeField] private LayoutElement[] _arrStageLayout;
    [SerializeField] private Image[] _arrStageItemImg;
    [SerializeField] private Button[] _arrStageItemBtn;
    [SerializeField] private Image[] _arrStageLockImg;
    [SerializeField] private Image _stageStartBtnBG;
    [SerializeField] private Button _stageStartBtn;
    [SerializeField] private Image _stageStartLabelImg;

    [Header("5. Setting Popup UI")]
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
    [SerializeField] private Image _controlChangeBtnImg;
    [SerializeField] private Button _controlChangeBtn;

    [Header("6. Mission Popup UI (Mission & Gold use Text)")]
    [SerializeField] private Image _missionPopupBG;
    [SerializeField] private Image _missionTitleImg;
    [SerializeField] private Image _missionCloseBtnImg;
    [SerializeField] private Button _missionCloseBtn;
    [SerializeField] private Image[] _arrMissionSlotBG;
    [SerializeField] private Text[] _arrMissionDetailTxt;
    [SerializeField] private Text[] _arrMissionProgressTxt;
    [SerializeField] private Text[] _arrMissionRewardGoldTxt;

    [Header("7. Stage Confirm & Notice Popup UI (All Image)")]
    [SerializeField] private Image _confirmPopupBG;
    [SerializeField] private Image _confirmMsgImg;
    [SerializeField] private Image _yesBtnImg;
    [SerializeField] private Button _yesBtn;
    [SerializeField] private Image _noBtnImg;
    [SerializeField] private Button _noBtn;
    [SerializeField] private Image _noticePopupBG;
    [SerializeField] private Image _noticeMsgImg;
    [SerializeField] private Image _noticeOkBtnImg;
    [SerializeField] private Button _noticeOkBtn;

    private EControlType _ePendingControlType = EControlType.TouchThreeSplit;
    private EGameModeType _eCurGameModeType = EGameModeType.Normal;
    private EDifficultyType _eCurDifficultyType = EDifficultyType.Easy;
    private int _selectedStageIndex = 0;
    private SaveLoadManager _saveLoadManager;
    private SaveLoadData _saveLoadData;

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
        if (_popupManager != null)
        {
            _popupManager.HideAllPopups();
        }

        bool isControlInit = UIManager.Instance != null && UIManager.Instance.IsControlInitialized();
        if (_windowManager != null)
        {
            _windowManager.ShowWindow(isControlInit ? EWindowType.Lobby : EWindowType.ControlSelect);
        }

        LoadGameData();
        UpdateModeButtonVisuals();
        RefreshStageSelection(0);

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionUpdate += RefreshMissionUI;
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionUpdate -= RefreshMissionUI;
        }
    }

    /// <summary> 세이브 데이터 및 로컬 UI 설정 로드 </summary>
    public void LoadGameData()
    {
        _saveLoadManager = new SaveLoadManager();
        _saveLoadData = _saveLoadManager.LoadData();

        if (_saveLoadData != null)
        {
            UpdateGoldText(_saveLoadData.Gold);
            RefreshSkinSlots();
            RefreshStageSlots();
        }

        RefreshMissionUI();
    }

    /// <summary> 골드 텍스트 갱신 (Text 사용) </summary>
    public void UpdateGoldText(int goldAmount)
    {
        if (_goldTxt != null)
        {
            _goldTxt.text = goldAmount.ToString("N0");
        }
    }

    /// <summary> 미션 팝업 실데이터 갱신 </summary>
    public void RefreshMissionUI()
    {
        if (MissionManager.Instance == null)
        {
            return;
        }

        int index = 0;
        foreach (MissionData data in MissionManager.Instance.GetDailyMissionList())
        {
            if (_arrMissionDetailTxt != null && index < _arrMissionDetailTxt.Length && _arrMissionDetailTxt[index] != null)
            {
                _arrMissionDetailTxt[index].text = MissionManager.Instance.GetMissionDetail(data.Id);
            }

            if (_arrMissionRewardGoldTxt != null && index < _arrMissionRewardGoldTxt.Length && _arrMissionRewardGoldTxt[index] != null)
            {
                _arrMissionRewardGoldTxt[index].text = data.Reward.ToString("N0") + " G";
            }
            index++;
        }

        int progressIdx = 0;
        foreach (MissionProgress progress in MissionManager.Instance.GetMissionProgress())
        {
            if (_arrMissionProgressTxt != null && progressIdx < _arrMissionProgressTxt.Length && _arrMissionProgressTxt[progressIdx] != null)
            {
                _arrMissionProgressTxt[progressIdx].text = progress.IsClear ? "완료!" : $"{progress.CurProgress}";
            }
            progressIdx++;
        }
    }

    /// <summary> 스킨 슬롯 해금 상태 갱신 </summary>
    private void RefreshSkinSlots()
    {
        if (_arrSkinLockImg == null)
        {
            return;
        }

        for (int i = 0; i < _arrSkinLockImg.Length; i++)
        {
            if (_arrSkinLockImg[i] == null)
            {
                continue;
            }

            // 기본 스킨(0번)은 항상 해금, 그 외는 세이브 데이터 확인
            bool isUnlocked = (i == 0);
            if (_saveLoadData != null && _saveLoadData.ListSkinSaveData != null && i < _saveLoadData.ListSkinSaveData.Count)
            {
                isUnlocked = isUnlocked || _saveLoadData.ListSkinSaveData[i].IsSkin;
            }

            _arrSkinLockImg[i].gameObject.SetActive(!isUnlocked);
        }
    }

    /// <summary> 스테이지 슬롯 잠금 상태 갱신 (1~16 스테이지) </summary>
    private void RefreshStageSlots()
    {
        if (_arrStageLockImg == null)
        {
            return;
        }

        for (int i = 0; i < _arrStageLockImg.Length; i++)
        {
            if (_arrStageLockImg[i] == null)
            {
                continue;
            }

            // 1스테이지(인덱스 0)는 항상 해금, 이후는 이전 스테이지 클리어 여부 확인
            bool isUnlocked = (i == 0);
            if (!isUnlocked && _saveLoadData != null && _saveLoadData.ListStageSaveData != null)
            {
                int prevIndex = i - 1;
                if (prevIndex < _saveLoadData.ListStageSaveData.Count)
                {
                    isUnlocked = _saveLoadData.ListStageSaveData[prevIndex].IsClear;
                }
            }

            _arrStageLockImg[i].gameObject.SetActive(!isUnlocked);
        }
    }

    private void BindButtons()
    {
        if (_arrControlBtn != null)
        {
            for (int i = 0; i < _arrControlBtn.Length; i++)
            {
                int index = i;
                if (_arrControlBtn[i] != null)
                {
                    _arrControlBtn[i].onClick.AddListener(() => OnClickControlOption((EControlType)index));
                }
            }
        }

        if (_confirmBtn != null)
        {
            _confirmBtn.onClick.AddListener(OnClickConfirmControl);
        }

        if (_lobbySettingBtn != null) _lobbySettingBtn.onClick.AddListener(() => _popupManager.ShowPopup(EPopupType.Setting));
        if (_skinSettingBtn != null) _skinSettingBtn.onClick.AddListener(() => _popupManager.ShowPopup(EPopupType.Setting));
        if (_stageSettingBtn != null) _stageSettingBtn.onClick.AddListener(() => _popupManager.ShowPopup(EPopupType.Setting));
        if (_settingCloseBtn != null) _settingCloseBtn.onClick.AddListener(() => _popupManager.HidePopup(EPopupType.Setting));

        if (_skinBtn != null) _skinBtn.onClick.AddListener(() => _windowManager.ShowWindow(EWindowType.Skin));
        if (_stageBtn != null) _stageBtn.onClick.AddListener(() => _windowManager.ShowWindow(EWindowType.Stage));
        if (_missionBtn != null) _missionBtn.onClick.AddListener(() => _popupManager.ShowPopup(EPopupType.Mission));
        if (_missionCloseBtn != null) _missionCloseBtn.onClick.AddListener(() => _popupManager.HidePopup(EPopupType.Mission));

        if (_skinBackBtn != null) _skinBackBtn.onClick.AddListener(() => _windowManager.ShowWindow(EWindowType.Lobby));
        if (_stageBackBtn != null) _stageBackBtn.onClick.AddListener(() => _windowManager.ShowWindow(EWindowType.Lobby));

        // 모드 선택 버튼 바인딩 (일반 모드 vs 무한 모드)
        if (_normalModeBtn != null)
        {
            _normalModeBtn.onClick.AddListener(() => OnClickModeButton(EGameModeType.Normal));
        }

        if (_infiniteModeBtn != null)
        {
            _infiniteModeBtn.onClick.AddListener(() => OnClickModeButton(EGameModeType.Infinite));
        }

        if (_controlChangeBtn != null)
        {
            _controlChangeBtn.onClick.AddListener(() =>
            {
                _popupManager.HidePopup(EPopupType.Setting);
                _windowManager.ShowWindow(EWindowType.ControlSelect);
            });
        }

        if (_arrSkinSlotBtn != null)
        {
            for (int i = 0; i < _arrSkinSlotBtn.Length; i++)
            {
                int slotIndex = i;
                if (_arrSkinSlotBtn[i] != null)
                {
                    _arrSkinSlotBtn[i].onClick.AddListener(() => OnClickSkinSlot(slotIndex));
                }
            }
        }

        if (_arrStageItemBtn != null)
        {
            for (int i = 0; i < _arrStageItemBtn.Length; i++)
            {
                int stageIdx = i;
                if (_arrStageItemBtn[i] != null)
                {
                    _arrStageItemBtn[i].onClick.AddListener(() => OnClickStageItem(stageIdx));
                }
            }
        }

        if (_stageStartBtn != null)
        {
            _stageStartBtn.onClick.AddListener(() => _popupManager.ShowPopup(EPopupType.StageConfirm));
        }

        if (_yesBtn != null)
        {
            _yesBtn.onClick.AddListener(OnClickStageConfirmYes);
        }

        if (_noBtn != null)
        {
            _noBtn.onClick.AddListener(() => _popupManager.HidePopup(EPopupType.StageConfirm));
        }

        if (_noticeOkBtn != null)
        {
            _noticeOkBtn.onClick.AddListener(() => _popupManager.HidePopup(EPopupType.Notice));
        }

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

    private void OnClickControlOption(EControlType controlType)
    {
        _ePendingControlType = controlType;
        int idx = (int)controlType;
        if (_arrControlPreviewSprite != null && idx >= 0 && idx < _arrControlPreviewSprite.Length && _arrControlPreviewSprite[idx] != null)
        {
            _controlPreviewImg.sprite = _arrControlPreviewSprite[idx];
        }
    }

    private void OnClickConfirmControl()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SaveControlType(_ePendingControlType);
        }

        _windowManager.ShowWindow(EWindowType.Lobby);
        _popupManager.ShowPopup(EPopupType.Notice);
    }

    private void OnClickSkinSlot(int slotIndex)
    {
        bool isLocked = _arrSkinLockImg != null && slotIndex < _arrSkinLockImg.Length && _arrSkinLockImg[slotIndex] != null && _arrSkinLockImg[slotIndex].gameObject.activeSelf;
        if (isLocked)
        {
            _popupManager.ShowPopup(EPopupType.Notice);
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SaveSelectedSkin(slotIndex);
        }
    }

    private void OnClickStageItem(int stageIndex)
    {
        bool isLocked = _arrStageLockImg != null && stageIndex < _arrStageLockImg.Length && _arrStageLockImg[stageIndex] != null && _arrStageLockImg[stageIndex].gameObject.activeSelf;
        if (isLocked)
        {
            _popupManager.ShowPopup(EPopupType.Notice);
            return;
        }

        RefreshStageSelection(stageIndex);
    }

    private void RefreshStageSelection(int selectedIndex)
    {
        _selectedStageIndex = selectedIndex;
        if (_arrStageLayout == null)
        {
            return;
        }

        for (int i = 0; i < _arrStageLayout.Length; i++)
        {
            if (_arrStageLayout[i] != null)
            {
                _arrStageLayout[i].preferredWidth = (i == selectedIndex) ? 520f : 100f;
            }
        }
    }

    /// <summary> 모드 버튼 클릭 처리 (일반 모드 vs 무한 모드) </summary>
    private void OnClickModeButton(EGameModeType modeType)
    {
        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Button);
        }
        */

        if (modeType == EGameModeType.Infinite)
        {
            // 무한 모드는 아직 제작되지 않았으므로 알림 팝업 노출
            _popupManager.ShowPopup(EPopupType.Notice);
            return;
        }

        _eCurGameModeType = modeType;
        UpdateModeButtonVisuals();

        // 일반 모드 선택 시 스테이지 스크롤 활성화
        if (_stageScroll != null)
        {
            _stageScroll.gameObject.SetActive(true);
        }
    }

    private void UpdateModeButtonVisuals()
    {
        if (_normalModeBtnImg != null)
        {
            _normalModeBtnImg.color = (_eCurGameModeType == EGameModeType.Normal) 
                ? new Color(0.05f, 0.65f, 0.98f, 1f) 
                : new Color(0.18f, 0.32f, 0.55f, 1f);
        }

        if (_infiniteModeBtnImg != null)
        {
            _infiniteModeBtnImg.color = (_eCurGameModeType == EGameModeType.Infinite)
                ? new Color(0.05f, 0.65f, 0.98f, 1f)
                : new Color(0.18f, 0.32f, 0.55f, 0.6f);
        }
    }

    private void OnClickStageConfirmYes()
    {
        _popupManager.HidePopup(EPopupType.StageConfirm);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SelectStageAndGameMode(_selectedStageIndex, _eCurGameModeType);
            UIManager.Instance.SelectStageAndDifficulty(_selectedStageIndex, _eCurDifficultyType);
        }

        SceneManager.LoadScene("02_Game");
    }
}
