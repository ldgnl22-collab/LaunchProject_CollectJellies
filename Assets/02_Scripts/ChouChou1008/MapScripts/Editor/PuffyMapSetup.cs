using System.IO;
using UnityEditor;
using UnityEngine;

/// 메뉴: Tools > Puffy World > Create Materials
/// 모바일 최적화 임포트 설정 + 테마별 Resources 폴더(스카이박스/땅/클레이/소품) + 테마 세트 생성
///
/// 결과 구조
///   PuffyWorldMaps/Resources/PuffyWorld/{테마}/Skybox.mat, Ground.mat, Clay.mat, Props/*.prefab
///   PuffyWorldMaps/PuffyThemes.asset
public static class PuffyMapSetup
{
    static readonly string[] Themes = { "01_SkyIsland", "02_Space", "03_Meteor", "04_StarryNight", "05_Ice", "06_City" };

    struct Look
    {
        public Color lip, shadow, sky, ground, rim, light, fog; public float rimAmt; public Vector3 lightEuler;
    }

    static Color C(float r, float g, float b) => new Color(r, g, b, 1);
    static Color C8(int r, int g, int b) => new Color32((byte)r, (byte)g, (byte)b, 255);

    static readonly Look[] Looks =
    {
        new Look { lip = C8(122,206,88),  shadow = C(.66f,.64f,.90f), sky = C(1f,1f,1.05f),    ground = C(.90f,.88f,1f),   rim = C(1f,1f,1f),     rimAmt = .35f, light = C(1f,.96f,.9f),  lightEuler = new Vector3(50,-35,0), fog = C8(214,230,255) },
        new Look { lip = C8(185,160,255), shadow = C(.45f,.38f,.80f), sky = C(.95f,.90f,1.1f),  ground = C(.70f,.60f,.95f), rim = C(.6f,.8f,1f),   rimAmt = .55f, light = C(1f,.92f,.88f), lightEuler = new Vector3(40,-30,0), fog = C8(70,45,140) },
        new Look { lip = C8(255,196,120), shadow = C(.62f,.45f,.75f), sky = C(1.05f,.9f,.95f),  ground = C(.85f,.70f,.90f), rim = C(1f,.75f,.65f),  rimAmt = .5f,  light = C(1f,.75f,.65f), lightEuler = new Vector3(20,180,0), fog = C8(255,186,170) },
        new Look { lip = C8(80,152,158),  shadow = C(.35f,.40f,.70f), sky = C(.75f,.82f,1.05f), ground = C(.55f,.60f,.85f), rim = C(.7f,.8f,1f),   rimAmt = .55f, light = C(.75f,.82f,1f), lightEuler = new Vector3(35,170,0), fog = C8(40,56,108) },
        new Look { lip = C8(246,251,255), shadow = C(.70f,.78f,.95f), sky = C(1f,1.03f,1.08f),  ground = C(.92f,.95f,1.05f), rim = C(.85f,.95f,1f), rimAmt = .4f,  light = C(1f,.98f,.95f), lightEuler = new Vector3(40,150,0), fog = C8(222,240,255) },
        new Look { lip = C8(150,222,200), shadow = C(.60f,.50f,.80f), sky = C(1.05f,.92f,.98f), ground = C(.85f,.75f,.92f), rim = C(1f,.8f,.75f),  rimAmt = .5f,  light = C(1f,.8f,.7f),   lightEuler = new Vector3(25,240,0), fog = C8(250,196,200) },
    };

    [MenuItem("Tools/Puffy World/Create Materials")]
    public static void Create()
    {
        string root = FindRoot();
        string resRoot = Ensure(Ensure(root, "Resources"), PuffyThemeSet.ResRoot);

        var groundShader = Shader.Find("PuffyWorld/Ground");
        var clayShader = Shader.Find("PuffyWorld/Clay");
        var skyShader = Shader.Find("Skybox/Cubemap");
        if (!groundShader || !clayShader || !skyShader)
        {
            Debug.LogError("셰이더를 찾을 수 없어요 (PuffyWorld/Ground, PuffyWorld/Clay, Skybox/Cubemap).");
            return;
        }

        var set = ScriptableObject.CreateInstance<PuffyThemeSet>();
        set.themes = new PuffyThemeSet.Theme[Themes.Length];
        int propCount = 0;

        try
        {
            for (int i = 0; i < Themes.Length; i++)
            {
                string t = Themes[i];
                var look = Looks[i];
                EditorUtility.DisplayProgressBar("Puffy World", t, (float)i / Themes.Length);
                string dir = Ensure(resRoot, t);

                // ── 스카이박스: 큐브맵 (파노라마 셰이더보다 픽셀당 계산이 훨씬 적고 밉맵 사용 가능)
                string skyPath = $"{root}/Textures/Sky/Sky_{t}.png";
                SetupSky(skyPath);
                var sky = LoadOrCreate($"{dir}/Skybox.mat", skyShader);
                sky.SetTexture("_Tex", AssetDatabase.LoadAssetAtPath<Cubemap>(skyPath));
                sky.SetFloat("_Exposure", 1);
                sky.SetFloat("_Rotation", 0);
                EditorUtility.SetDirty(sky);

                // ── 땅
                string g = $"{root}/Textures/Ground/Ground_{t}";
                SetupTex(g + "_Top.png", false);
                SetupTex(g + "_Side.png", false);
                SetupTex(g + "_Top_N.png", true);
                SetupTex(g + "_Side_N.png", true);
                var ground = LoadOrCreate($"{dir}/Ground.mat", groundShader);
                ground.SetTexture("_TopTex", Tex(g + "_Top.png"));
                ground.SetTexture("_SideTex", Tex(g + "_Side.png"));
                ground.SetTexture("_TopNormal", Tex(g + "_Top_N.png"));
                ground.SetTexture("_SideNormal", Tex(g + "_Side_N.png"));
                ground.SetColor("_LipColor", look.lip);
                ApplyLook(ground, look);
                EditorUtility.SetDirty(ground);

                // ── 클레이 (소품 / 테두리) — GPU 인스턴싱 켬
                var clay = LoadOrCreate($"{dir}/Clay.mat", clayShader);
                clay.enableInstancing = true;
                ApplyLook(clay, look);
                EditorUtility.SetDirty(clay);

                // ── 소품 프리팹
                string modelDir = $"{root}/Models/{t}";
                if (AssetDatabase.IsValidFolder(modelDir))
                {
                    string pDir = Ensure(dir, "Props");
                    foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { modelDir }))
                    {
                        string mp = AssetDatabase.GUIDToAssetPath(guid);
                        SetupModel(mp);
                        var model = AssetDatabase.LoadAssetAtPath<GameObject>(mp);
                        if (!model) continue;
                        var inst = (GameObject)PrefabUtility.InstantiatePrefab(model);
                        foreach (var r in inst.GetComponentsInChildren<MeshRenderer>())
                        {
                            r.sharedMaterial = clay;
                            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            r.receiveShadows = false;
                            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                        }
                        PrefabUtility.SaveAsPrefabAsset(inst, $"{pDir}/{Path.GetFileNameWithoutExtension(mp)}.prefab");
                        Object.DestroyImmediate(inst);
                        propCount++;
                    }
                }

                set.themes[i] = new PuffyThemeSet.Theme
                {
                    name = t, edgeColor = look.lip, lightColor = look.light, lightEuler = look.lightEuler, fogColor = look.fog
                };
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        string setPath = $"{root}/PuffyThemes.asset";
        AssetDatabase.DeleteAsset(setPath);
        AssetDatabase.CreateAsset(set, setPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = set;
        Debug.Log($"Puffy World: 테마 {Themes.Length}개, 소품 프리팹 {propCount}개 생성 완료 (모바일 설정 적용) → {resRoot}");
    }

    static void ApplyLook(Material m, Look l)
    {
        m.SetColor("_ShadowColor", l.shadow);
        m.SetColor("_SkyAmbient", l.sky);
        m.SetColor("_GroundAmbient", l.ground);
        m.SetColor("_RimColor", l.rim);
        m.SetFloat("_RimStrength", l.rimAmt);
        Vector3 d = Quaternion.Euler(l.lightEuler) * Vector3.back;
        m.SetVector("_LightDir", new Vector4(d.x, d.y, d.z, 0));
    }

    // ───────────── 임포트 설정 ─────────────
    static void Mobile(TextureImporter imp, int size, TextureImporterFormat fmt)
    {
        foreach (var plat in new[] { "Android", "iPhone" })
        {
            var s = imp.GetPlatformTextureSettings(plat);
            s.overridden = true; s.maxTextureSize = size; s.format = fmt;
            imp.SetPlatformTextureSettings(s);
        }
    }

    static void SetupSky(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!imp) { Debug.LogWarning("텍스처 없음: " + path); return; }
        imp.textureType = TextureImporterType.Default;
        imp.textureShape = TextureImporterShape.TextureCube;
        imp.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;   // 위경도(파노라마) → 큐브맵
        imp.sRGBTexture = true;
        imp.mipmapEnabled = false;                                        // 스카이박스: 밉맵 불필요
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.filterMode = FilterMode.Bilinear;
        imp.maxTextureSize = 256;                                          // 면 하나 크기 (모바일)
        imp.textureCompression = TextureImporterCompression.CompressedHQ;
        Mobile(imp, 256, TextureImporterFormat.ASTC_6x6);                  // 폰: 면 256
        imp.SaveAndReimport();
    }

    static void SetupTex(string path, bool normal)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!imp) { Debug.LogWarning("텍스처 없음: " + path); return; }
        imp.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        imp.textureShape = TextureImporterShape.Texture2D;
        imp.mipmapEnabled = true;
        imp.streamingMipmaps = false;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.filterMode = FilterMode.Bilinear;
        imp.anisoLevel = 2;
        imp.maxTextureSize = 256;
        imp.textureCompression = TextureImporterCompression.Compressed;
        Mobile(imp, 256, normal ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_6x6);
        imp.SaveAndReimport();
    }

    static void SetupModel(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (!imp) return;
        imp.materialImportMode = ModelImporterMaterialImportMode.None;
        imp.importAnimation = false;
        imp.animationType = ModelImporterAnimationType.None;
        imp.importCameras = false; imp.importLights = false;
        imp.importBlendShapes = false;
        imp.importTangents = ModelImporterTangents.None;     // 클레이 셰이더는 접선 불필요
        imp.importNormals = ModelImporterNormals.Import;
        imp.isReadable = false;                              // 모바일: CPU 사본 제거 (런타임 메시 합치기를 쓸 때만 true)
        imp.meshCompression = ModelImporterMeshCompression.Medium;
        imp.optimizeMeshVertices = true;
        imp.optimizeMeshPolygons = true;
        imp.addCollider = false;
        imp.SaveAndReimport();
    }

    // ───────────── 도구 ─────────────
    static Texture2D Tex(string p) => AssetDatabase.LoadAssetAtPath<Texture2D>(p);

    static Material LoadOrCreate(string path, Shader shader)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m) { m.shader = shader; return m; }
        m = new Material(shader);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static string Ensure(string parent, string name)
    {
        string p = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(p)) AssetDatabase.CreateFolder(parent, name);
        return p;
    }

    static string FindRoot()
    {
        var guid = AssetDatabase.FindAssets("PuffyMapSetup t:MonoScript")[0];
        var p = AssetDatabase.GUIDToAssetPath(guid);
        return Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\', '/');
    }
}
