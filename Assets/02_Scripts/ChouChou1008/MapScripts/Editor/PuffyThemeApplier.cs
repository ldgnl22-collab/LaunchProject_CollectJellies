using UnityEngine;

/// 테마 번호만 바꾸면 스카이박스 + 땅 + 조명 + 3D 소품 + 테두리를 한 번에 적용.
///
/// 모바일 최적화 흐름
/// 1) 에디터에서 Theme Index 를 고르면 그 자리에서 미리 "구워" 씬에 저장됨
///    (길 메시 합치기, 테두리, 소품 → 게임 시작할 때 계산 0)
/// 2) 게임 중 SetTheme() 로 바꿀 때만 다시 굽고, 안 쓰는 테마 에셋은 메모리에서 내림
[ExecuteAlways]
[RequireComponent(typeof(PuffyEdgeBuilder), typeof(PuffyPropScatter))]
public class PuffyThemeApplier : MonoBehaviour
{
    public PuffyThemeSet themeSet;
    [Range(0, 5)] public int themeIndex = 0;

    [Header("대상")]
    [Tooltip("길(땅) 오브젝트들의 부모")]
    public Transform groundRoot;
    [Tooltip("씬의 Directional Light")]
    public Light mainLight;

    [Header("옵션")]
    [Tooltip("길 블록들을 메시 몇 개로 합침 (드로우콜 대폭 감소). 원본은 콜라이더용으로 남고 렌더러만 꺼짐")]
    public bool combineGround = true;
    public bool buildEdges = true;
    public bool scatterProps = true;
    public bool applyLight = true;
    [Tooltip("실시간 그림자 끄기 (모바일 권장, 젤리엔 동그란 그림자 사용)")]
    public bool disableRealtimeShadows = true;
    public bool useFog = true;
    public float fogStart = 60f, fogEnd = 220f;

    [SerializeField, HideInInspector] int bakedTheme = -1;
    static readonly int SurfaceY = Shader.PropertyToID("_SurfaceY");

    void Start()
    {
        // 에디터에서 안 구운 상태로 실행했을 때만 런타임에 굽기
        if (Application.isPlaying && bakedTheme != themeIndex) Apply();
    }

    void Update()
    {
        // 에디터에서만: 번호를 바꾸면 즉시 굽기
        if (!Application.isPlaying && bakedTheme != themeIndex) Apply();
    }

    /// 게임 중 다음 맵(테마)으로 바꿀 때 호출
    public void SetTheme(int index)
    {
        if (index == bakedTheme) return;
        themeIndex = index;
        Apply();
        Resources.UnloadUnusedAssets();     // 이전 테마 텍스처/메시 해제
    }

    [ContextMenu("Rebuild (다시 굽기)")]
    public void Rebuild() { bakedTheme = -1; Apply(); }

    [ContextMenu("Apply Theme")]
    public void Apply()
    {
        if (!themeSet || themeSet.themes == null || themeSet.themes.Length == 0) return;
        themeIndex = Mathf.Clamp(themeIndex, 0, themeSet.themes.Length - 1);
        var t = themeSet.themes[themeIndex];
        var sky = t.LoadSkybox();
        var ground = t.LoadGround();
        var clay = t.LoadClay();
        if (!ground || !clay)
        {
            Debug.LogWarning($"[Puffy] Resources/{PuffyThemeSet.ResRoot}/{t.name} 에서 머티리얼을 찾을 수 없어요. Tools > Puffy World > Create Materials 를 먼저 실행하세요.");
            return;
        }

        if (sky) RenderSettings.skybox = sky;
        RenderSettings.fog = useFog;
        if (useFog)
        {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = t.fogColor;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;   // 환경광 계산 생략 (셰이더가 자체 처리)

        if (mainLight)
        {
            if (applyLight)
            {
                mainLight.color = t.lightColor;
                mainLight.transform.rotation = Quaternion.Euler(t.lightEuler);
            }
            if (disableRealtimeShadows) mainLight.shadows = LightShadows.None;
            if (!mainLight.GetComponent<PuffyLightSync>()) mainLight.gameObject.AddComponent<PuffyLightSync>();
        }

        if (groundRoot)
        {
            var sources = PuffyMeshUtil.Sources(groundRoot);
            BakeGround(sources, ground);
            var eb = GetComponent<PuffyEdgeBuilder>();
            if (buildEdges) eb.Build(groundRoot, sources, clay, t.edgeColor);
            else PuffyMeshUtil.DestroyChild(groundRoot, PuffyMeshUtil.EdgeName);
            var sc = GetComponent<PuffyPropScatter>();
            if (scatterProps) sc.Build(groundRoot, sources, t.LoadProps(), clay);
            else PuffyMeshUtil.DestroyChild(transform, PuffyMeshUtil.PropsName);
        }
        bakedTheme = themeIndex;
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    void BakeGround(System.Collections.Generic.List<MeshFilter> sources, Material ground)
    {
        PuffyMeshUtil.DestroyChild(groundRoot, PuffyMeshUtil.GroundName);
        if (combineGround)
        {
            var items = new System.Collections.Generic.List<PuffyMeshUtil.Item>();
            foreach (var mf in sources)
            {
                var top = PuffyMeshUtil.WorldBounds(mf).max.y;
                items.Add(new PuffyMeshUtil.Item { mesh = mf.sharedMesh, matrix = mf.transform.localToWorldMatrix, hasUv3 = true, uv3 = new Vector2(top, 1) });
                var r = mf.GetComponent<MeshRenderer>();
                if (r) r.enabled = false;            // 콜라이더는 그대로, 그리기만 끔
            }
            PuffyMeshUtil.BuildChunks(items, 80f, groundRoot, ground, PuffyMeshUtil.GroundName);
        }
        else
        {
            var block = new MaterialPropertyBlock();
            foreach (var mf in sources)
            {
                var r = mf.GetComponent<MeshRenderer>();
                if (!r) continue;
                r.enabled = true;
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = ground;
                r.sharedMaterials = mats;
                r.GetPropertyBlock(block);
                block.SetFloat(SurfaceY, PuffyMeshUtil.WorldBounds(mf).max.y);
                r.SetPropertyBlock(block);
            }
        }
    }
}
