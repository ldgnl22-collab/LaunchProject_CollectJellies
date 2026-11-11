using UnityEngine;
using UnityEngine.UI;

/// <summary> 전체 화면(Window) 전환 및 로딩 화면을 관리하는 매니저 </summary>
public class WindowManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private GameObject _titleWindowObj;
    [SerializeField] private GameObject _controlSelectWindowObj;
    [SerializeField] private GameObject _lobbyWindowObj;
    [SerializeField] private GameObject _skinWindowObj;
    [SerializeField] private GameObject _stageWindowObj;
    [SerializeField] private GameObject _inGameWindowObj;

    [Header("Loading UI")]
    [SerializeField] private GameObject _loadingWindowObj;
    [SerializeField] private Image _loadingGaugeImg;

    private EWindowType _eCurWindowType = EWindowType.None;
    public EWindowType ECurWindowType => _eCurWindowType;

    /// <summary> 지정한 윈도우를 활성화하고 나머지 윈도우를 비활성화 </summary>
    public void ShowWindow(EWindowType windowType)
    {
        _eCurWindowType = windowType;

        if (_titleWindowObj != null) _titleWindowObj.SetActive(windowType == EWindowType.Title);
        if (_controlSelectWindowObj != null) _controlSelectWindowObj.SetActive(windowType == EWindowType.ControlSelect);
        if (_lobbyWindowObj != null) _lobbyWindowObj.SetActive(windowType == EWindowType.Lobby);
        if (_skinWindowObj != null) _skinWindowObj.SetActive(windowType == EWindowType.Skin);
        if (_stageWindowObj != null) _stageWindowObj.SetActive(windowType == EWindowType.Stage);
        if (_inGameWindowObj != null) _inGameWindowObj.SetActive(windowType == EWindowType.InGame);
    }

    /// <summary> 로딩 화면 표시 </summary>
    public void ShowLoading()
    {
        if (_loadingWindowObj != null && !_loadingWindowObj.activeSelf)
        {
            _loadingWindowObj.SetActive(true);
        }
    }

    /// <summary> 로딩 진행도 갱신 </summary>
    public void SetLoadingProgress(float progress)
    {
        if (_loadingGaugeImg != null)
        {
            _loadingGaugeImg.fillAmount = Mathf.Clamp01(progress);
        }
    }

    /// <summary> 로딩 화면 숨김 </summary>
    public void HideLoading()
    {
        if (_loadingWindowObj != null && _loadingWindowObj.activeSelf)
        {
            _loadingWindowObj.SetActive(false);
        }
    }
}
