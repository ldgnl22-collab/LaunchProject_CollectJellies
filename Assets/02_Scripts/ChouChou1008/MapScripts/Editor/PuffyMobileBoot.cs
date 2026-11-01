using UnityEngine;
using UnityEngine.Rendering;

/// 모바일 성능 설정 — 첫 씬의 빈 오브젝트에 하나만 붙이면 됨 (씬이 바뀌어도 유지)
/// · 목표 FPS 고정 (배터리·발열 감소)
/// · 렌더 해상도 자동 조절: 고해상도 폰은 픽셀 수가 너무 많아서 가장 큰 발열 원인 → DPI 기준으로 낮춤
/// · 품질 단계 자동 선택 (High / Low 셰이더) + 프레임이 계속 떨어지면 자동으로 Low 로 전환
public class PuffyMobileBoot : MonoBehaviour
{
    public enum Tier { Auto, High, Low }

    [Header("프레임")]
    [Tooltip("30 = 배터리·발열 최소, 60 = 부드러움")]
    public int targetFps = 60;

    [Header("해상도")]
    public bool autoRenderScale = true;
    [Tooltip("이 DPI 보다 높은 화면은 해상도를 낮춤 (작은 폰 화면에선 차이가 거의 안 보임)")]
    public float targetDpi = 300f;
    [Range(0.5f, 1f)] public float manualRenderScale = 1f;
    [Range(0.5f, 1f)] public float minRenderScale = 0.6f;

    [Header("품질")]
    public Tier quality = Tier.Auto;
    [Tooltip("실시간 그림자 전체 끄기 (모바일 권장 — 캐릭터엔 동그란 그림자 사용)")]
    public bool disableShadows = true;
    [Tooltip("평균 FPS가 목표의 80% 아래로 계속 떨어지면 Low 로 자동 전환")]
    public bool adaptive = true;

    public static Tier CurrentTier { get; private set; } = Tier.High;
    public const int HighLod = 300, LowLod = 150;

    static PuffyMobileBoot instance;
    float avgDt, slowTime;

    void Awake()
    {
        if (instance && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFps;
        if (disableShadows) QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.pixelLightCount = 1;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

        SetTier(quality == Tier.Auto ? DetectTier() : quality);
        ApplyRenderScale();
        avgDt = 1f / targetFps;
    }

    static Tier DetectTier()
    {
        bool weak = SystemInfo.systemMemorySize < 3500
                 || SystemInfo.graphicsMemorySize < 1024
                 || SystemInfo.processorCount <= 4
                 || SystemInfo.graphicsShaderLevel < 35;
        return weak ? Tier.Low : Tier.High;
    }

    public static void SetTier(Tier t)
    {
        CurrentTier = t == Tier.Low ? Tier.Low : Tier.High;
        Shader.globalMaximumLOD = CurrentTier == Tier.High ? HighLod : LowLod;
    }

    void ApplyRenderScale()
    {
        float scale = manualRenderScale;
        if (autoRenderScale && Screen.dpi > 1f)
            scale = Mathf.Min(scale, targetDpi / Screen.dpi);
        scale = Mathf.Clamp(scale, minRenderScale, 1f);

        var rp = GraphicsSettings.currentRenderPipeline;
        if (rp != null)
        {
            // URP: renderScale 속성 사용
            var prop = rp.GetType().GetProperty("renderScale");
            if (prop != null && prop.CanWrite) prop.SetValue(rp, scale);
        }
        else if (Application.isMobilePlatform && scale < 0.99f)
        {
            // Built-in: 백버퍼 해상도 자체를 낮춤
            int w = Mathf.RoundToInt(Display.main.systemWidth * scale);
            int h = Mathf.RoundToInt(Display.main.systemHeight * scale);
            Screen.SetResolution(w, h, true);
        }
    }

    void Update()
    {
        if (!adaptive || CurrentTier == Tier.Low) return;
        avgDt = Mathf.Lerp(avgDt, Time.unscaledDeltaTime, 0.05f);
        if (1f / avgDt < targetFps * 0.8f) slowTime += Time.unscaledDeltaTime;
        else slowTime = Mathf.Max(0, slowTime - Time.unscaledDeltaTime);
        if (slowTime > 4f)
        {
            SetTier(Tier.Low);
            Debug.Log("[Puffy] FPS가 낮아서 Low 품질로 전환했어요.");
        }
    }
}
