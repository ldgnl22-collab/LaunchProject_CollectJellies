using UnityEngine;

/// <summary> 팝업(PopUp) 창 활성화 및 비활성화를 관리하는 매니저 </summary>
public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingPopupObj;
    [SerializeField] private GameObject _missionPopupObj;
    [SerializeField] private GameObject _stageConfirmPopupObj;
    [SerializeField] private GameObject _noticePopupObj;
    [SerializeField] private GameObject _clearResultPopupObj;
    [SerializeField] private GameObject _failResultPopupObj;

    /// <summary> 지정한 팝업창 열기 </summary>
    public void ShowPopup(EPopupType popupType)
    {
        GameObject targetObj = GetPopupObject(popupType);
        if (targetObj != null)
        {
            targetObj.SetActive(true);
        }
    }

    /// <summary> 지정한 팝업창 닫기 </summary>
    public void HidePopup(EPopupType popupType)
    {
        GameObject targetObj = GetPopupObject(popupType);
        if (targetObj != null)
        {
            targetObj.SetActive(false);
        }
    }

    /// <summary> 모든 팝업창 닫기 </summary>
    public void HideAllPopups()
    {
        if (_settingPopupObj != null) _settingPopupObj.SetActive(false);
        if (_missionPopupObj != null) _missionPopupObj.SetActive(false);
        if (_stageConfirmPopupObj != null) _stageConfirmPopupObj.SetActive(false);
        if (_noticePopupObj != null) _noticePopupObj.SetActive(false);
        if (_clearResultPopupObj != null) _clearResultPopupObj.SetActive(false);
        if (_failResultPopupObj != null) _failResultPopupObj.SetActive(false);
    }

    private GameObject GetPopupObject(EPopupType popupType)
    {
        switch (popupType)
        {
            case EPopupType.Setting:
                return _settingPopupObj;
            case EPopupType.Mission:
                return _missionPopupObj;
            case EPopupType.StageConfirm:
                return _stageConfirmPopupObj;
            case EPopupType.Notice:
                return _noticePopupObj;
            case EPopupType.ClearResult:
                return _clearResultPopupObj;
            case EPopupType.FailResult:
                return _failResultPopupObj;
            default:
                return null;
        }
    }
}
