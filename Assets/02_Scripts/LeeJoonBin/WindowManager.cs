using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private Canvas _titleCanvas;
    [SerializeField] private CanvasGroup _loadingCanvas;
    
    public void ShowLoading()
    {
        _titleCanvas.enabled = false;
        _loadingCanvas.enabled = true;
        
    }
    public void SetLoadingProgress(float progress)
    {
    }

    public void HideLoading()
    {
        
    }
}
