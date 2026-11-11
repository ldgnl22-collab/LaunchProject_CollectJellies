using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> 타이틀 씬(00_Tilte) UI 컨트롤러 </summary>
public class TitleSceneUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private PopupManager _popupManager;

    [Header("Title Window UI (All Image)")]
    [SerializeField] private Image _titleBG;
    [SerializeField] private Image _titleLogoImg;
    [SerializeField] private Image _gameStartBG;
    [SerializeField] private Button _gameStartBtn;
    [SerializeField] private Image _gameStartLabelImg;

    private void Awake()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterSceneManagers(_windowManager, _popupManager);
        }

        if (_gameStartBtn != null)
        {
            _gameStartBtn.onClick.AddListener(OnClickGameStart);
        }
    }

    private void Start()
    {
        if (_windowManager != null)
        {
            _windowManager.ShowWindow(EWindowType.Title);
        }
    }

    /// <summary> 게임 시작 버튼 클릭 시 로딩 연출과 함께 로비 씬으로 이동 </summary>
    private void OnClickGameStart()
    {
        // 사운드 실행 임시 주석 처리
        /*
        try
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Button);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TitleSceneUI] SoundManager 재생 실패: {ex.Message}");
        }
        */

        StartCoroutine(CoLoadLobby());
    }

    private System.Collections.IEnumerator CoLoadLobby()
    {
        if (_windowManager != null)
        {
            _windowManager.ShowLoading();
            _windowManager.SetLoadingProgress(0.1f);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync("01_Lobby");
        if (op == null)
        {
            SceneManager.LoadScene("01_Lobby");
            yield break;
        }

        op.allowSceneActivation = false;

        float progress = 0.1f;
        while (op.progress < 0.9f)
        {
            progress = Mathf.MoveTowards(progress, op.progress, Time.deltaTime);
            if (_windowManager != null)
            {
                _windowManager.SetLoadingProgress(progress);
            }
            yield return null;
        }

        while (progress < 1f)
        {
            progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime * 2f);
            if (_windowManager != null)
            {
                _windowManager.SetLoadingProgress(progress);
            }
            yield return null;
        }

        if (_windowManager != null)
        {
            _windowManager.HideLoading();
        }

        op.allowSceneActivation = true;
    }
}
