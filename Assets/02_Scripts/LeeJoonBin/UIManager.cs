using UnityEngine;

public class UIManager :  Singleton<UIManager>
{
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private PopupManager _popupManager;
    
    public static UIManager Instance { get; private set; }
    
    public WindowManager WindowManager => _windowManager;
    public PopupManager PopupManager => _popupManager;

    private void Awake() => SetSingleton();
    private void OnEnable() => BindLoadingEvents();
    private void OnDisable() => UnbindLoadingEvents();

    private void BindLoadingEvents()
    {
        GameManager.Instance.OnProgressChanged += HandleProgressChanged;
        GameManager.Instance.OnLoadingComplete += HandleLoadingComplete;
    }

    private void UnbindLoadingEvents()
    {
        GameManager.Instance.OnProgressChanged -= HandleProgressChanged;
        GameManager.Instance.OnLoadingComplete -= HandleLoadingComplete;
    }

    private void HandleProgressChanged(float progress)
    {
        
        _windowManager.ShowLoading();
        _windowManager.SetLoadingProgress(progress);
    }

    private void HandleLoadingComplete()
    {
        _windowManager.HideLoading();
    }
}