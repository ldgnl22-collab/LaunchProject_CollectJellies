using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GravityCollect.EditorTools
{
    /// <summary>
    /// Tools > Gravity Collect > Build Slime Prefabs
    /// Finds the slime files wherever they were imported, sets up the face atlas, creates materials
    /// (Built-in render pipeline shaders only — no URP), one prefab per colour
    /// (Slime_Pink, Slime_Blue, ...) and a three-slime tower (Slime_Stack), plus the Idle / Move / Jump / Death
    /// animation clips and an Animator Controller that every prefab uses.
    /// Shows a dialog with the result, and the reason if anything failed.
    /// </summary>
    public static class SlimePrefabBuilder
    {
        static readonly (string name, string top, string bottom, SlimeFace face)[] Palette =
        {
            ("Pink",   "#FFD0E4", "#FF5C9D", SlimeFace.Smile),
            ("Blue",   "#C4E8FF", "#3D86FF", SlimeFace.Happy),
            ("Green",  "#D6FBB4", "#3FBF5A", SlimeFace.Laugh),
            ("Orange", "#FFE6B0", "#FF8A2B", SlimeFace.Wink),
            ("Purple", "#E7D2FF", "#8A52FF", SlimeFace.Love),
            ("Mint",   "#CBFFF1", "#20C9A6", SlimeFace.Starry),
            ("Yellow", "#FFF7B8", "#FFBF1A", SlimeFace.Shy),
            ("Red",    "#FFD0C8", "#FF4D5E", SlimeFace.Surprised),
            ("Sky",    "#D4F6FF", "#29B6E8", SlimeFace.Sad),
            ("Magenta","#F9D8FF", "#D24FE0", SlimeFace.Angry),
            ("Lime",   "#EAFFB8", "#9BD61C", SlimeFace.Dizzy),
            ("Peach",  "#FFE3D0", "#FF9A6B", SlimeFace.Sleepy),
            ("Gold",   "#FFF4B0", "#E09A16", SlimeFace.Starry),
            ("Brown",  "#E8C49A", "#8A5530", SlimeFace.Cat),
            ("Gray",   "#E6E8EE", "#8C90A0", SlimeFace.Smile),
            ("White",  "#FFFFFF", "#D8D6E8", SlimeFace.Happy),
            ("Black",  "#6E6688", "#1C1826", SlimeFace.Cheer),
        };

        static Material MatFrom(string root, string name, Shader shader, SlimeFriendsData.MatDef d, bool fallback)
        {
            Material m = LoadOrCreate(root, name, shader);
            if (m.HasProperty("_TopColor")) m.SetColor("_TopColor", Hex(d.top));
            if (m.HasProperty("_BottomColor")) m.SetColor("_BottomColor", Hex(d.bottom));
            if (m.HasProperty("_GradientBase")) m.SetFloat("_GradientBase", d.g0);
            if (m.HasProperty("_GradientHeight")) m.SetFloat("_GradientHeight", d.g1);
            if (m.HasProperty("_SelfGlow")) m.SetFloat("_SelfGlow", d.glow);
            if (m.HasProperty("_EdgeGlow")) m.SetFloat("_EdgeGlow", d.edge);
            if (m.HasProperty("_PatternTex"))
            {
                Texture2D pat = null;
                if (!string.IsNullOrEmpty(d.pattern))
                {
                    string pp = Find("Pattern_" + d.pattern + " t:Texture2D", "Pattern_" + d.pattern + ".png");
                    if (pp != null)
                    {
                        var ti = AssetImporter.GetAtPath(pp) as TextureImporter;
                        if (ti != null && ti.wrapMode != TextureWrapMode.Repeat) { ti.wrapMode = TextureWrapMode.Repeat; ti.SaveAndReimport(); }
                        pat = AssetDatabase.LoadAssetAtPath<Texture2D>(pp);
                    }
                }
                m.SetTexture("_PatternTex", pat);
                m.SetColor("_PatternColor", pat != null ? Hex(d.patColor) : Color.black);
                m.SetFloat("_PatternScale", d.patScale);
                m.SetVector("_PatternScroll", new Vector4(d.scroll.x, d.scroll.y, 0f, 0f));
            }
            if (fallback)
            {
                Color mid = Color.Lerp(Hex(d.top), Hex(d.bottom), 0.5f);
                if (m.HasProperty("_Color")) m.SetColor("_Color", mid);
            }
            m.enableInstancing = true;
            EditorUtility.SetDirty(m);
            return m;
        }

        [MenuItem("Tools/Gravity Collect/Build Slime Prefabs (with Animations)")]
        public static void BuildFromMenu() => Build(true);

        public static void Build() => Build(true);

        public static bool Build(bool showDialog)
        {
            var log = new StringBuilder();
            try
            {
                // 1. Find the files wherever the folder ended up.
                string modelPath = Find("DomeSlime t:Model", "DomeSlime.obj");
                if (modelPath == null) return Fail("DomeSlime.obj 모델을 프로젝트에서 찾을 수 없어요. 패키지를 다시 Import 해 주세요.", showDialog);
                string root = Path.GetDirectoryName(Path.GetDirectoryName(modelPath)).Replace('\\', '/');   // .../Slime
                string facePath = Find("SlimeFaces t:Texture2D", "SlimeFaces.png");
                log.AppendLine("폴더: " + root);

                SetupFaceTexture(facePath);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                Texture2D faces = facePath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(facePath) : null;
                if (model == null) return Fail("모델 파일은 있지만 불러올 수 없어요: " + modelPath, showDialog);
                if (faces == null) log.AppendLine("⚠ 표정 텍스처(SlimeFaces.png)를 못 찾아서 얼굴 없이 만들어요.");

                // 2. Shaders: Built-in render pipeline only (no URP), with light mobile fallbacks.
                if (GraphicsSettings.currentRenderPipeline != null)
                    log.AppendLine("⚠ 프로젝트가 아직 URP예요. Tools ▸ Gravity Collect ▸ Switch Project to Built-in 을 눌러야 젤리가 보라색으로 안 보여요.");
                Shader body = PickShader("GravityCollect/Slime Body (Built-in)", "SlimeBuiltin", log);
                bool bodyFallback = false;
                if (body == null)
                {
                    body = Shader.Find("Mobile/Diffuse");
                    bodyFallback = true;
                    log.AppendLine("⚠ 슬라임 몸통 셰이더를 쓸 수 없어 Mobile/Diffuse 로 대신 만들었어요 (그라데이션 없음).");
                }
                Shader faceShader = PickShader("GravityCollect/Slime Face", "SlimeFace", log);
                if (faceShader == null)
                {
                    faceShader = Shader.Find("Unlit/Transparent");
                    log.AppendLine("⚠ 얼굴 셰이더를 쓸 수 없어 Unlit/Transparent 로 대신했어요 (표정 전환 안 됨).");
                }
                if (body == null || faceShader == null) return Fail("사용할 수 있는 셰이더가 하나도 없어요.\n" + log, showDialog);

                EnsureFolder(root + "/Materials");
                EnsureFolder(root + "/Prefabs");

                Material face = LoadOrCreate(root, "Slime_Face", faceShader);
                if (faces != null) { face.mainTexture = faces; if (face.HasProperty("_MainTex")) face.SetTexture("_MainTex", faces); if (face.HasProperty("_BaseMap")) face.SetTexture("_BaseMap", faces); }
                if (face.HasProperty("_Rows")) face.SetFloat("_Rows", 5f);
                if (face.HasProperty("_Columns")) face.SetFloat("_Columns", 4f);
                face.enableInstancing = true;
                EditorUtility.SetDirty(face);

                AnimatorController controller = null;
                try { controller = BuildAnimations(root); log.AppendLine("애니메이션: Idle, Move, Jump, Death → " + root + "/Animations"); }
                catch (Exception e) { log.AppendLine("⚠ 애니메이션을 만들지 못했어요: " + e.Message); }

                Material droplet = null;
                string dropPath = Find("SlimeDroplet t:Texture2D", "SlimeDroplet.png");
                Shader dropShader = PickShader("GravityCollect/Slime Droplet", "SlimeDroplet", log);
                if (dropShader != null)
                {
                    SetupFaceTexture(dropPath);
                    droplet = LoadOrCreate(root, "Slime_Droplet", dropShader);
                    if (dropPath != null) droplet.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(dropPath);
                    EditorUtility.SetDirty(droplet);
                }

                var made = new Dictionary<string, GameObject>();
                foreach (var p in Palette)
                {
                    Color top = Hex(p.top), bottom = Hex(p.bottom);
                    Material m = LoadOrCreate(root, "Slime_Body_" + p.name, body);
                    if (m.HasProperty("_TopColor")) m.SetColor("_TopColor", top);
                    if (m.HasProperty("_BottomColor")) m.SetColor("_BottomColor", bottom);
                    if (bodyFallback)
                    {
                        Color mid = Color.Lerp(top, bottom, 0.5f);
                                if (m.HasProperty("_Color")) m.SetColor("_Color", mid);
                        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.85f);
                        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.85f);
                    }
                    m.enableInstancing = true;
                    EditorUtility.SetDirty(m);

                    if (p.name == "Gold" && m.HasProperty("_PatternTex"))   // gold gets a little sparkle
                    {
                        string sp = Find("Pattern_Sparkle t:Texture2D", "Pattern_Sparkle.png");
                        if (sp != null) { m.SetTexture("_PatternTex", AssetDatabase.LoadAssetAtPath<Texture2D>(sp)); m.SetColor("_PatternColor", Color.white); m.SetFloat("_PatternScale", 1.4f); }
                        if (m.HasProperty("_SelfGlow")) m.SetFloat("_SelfGlow", 0.12f);
                    }
                    Material faceMat = face;
                    if (p.name == "Black")   // white eyes/lines on the dark body
                    {
                        faceMat = LoadOrCreate(root, "Slime_Face_Light", face.shader);
                        faceMat.CopyPropertiesFromMaterial(face);
                        if (faceMat.HasProperty("_InkColor")) faceMat.SetColor("_InkColor", new Color(1f, 0.97f, 1f));
                        EditorUtility.SetDirty(faceMat);
                    }
                    GameObject prefab = SavePrefab(root, "Slime_" + p.name, model, m, faceMat, top, bottom, p.face, controller, droplet);
                    if (prefab != null) made[p.name] = prefab;
                }

                foreach (var fr in SlimeFriendsData.Jellies)
                {
                    string fpath = Find(fr.model + " t:Model", fr.model + ".obj");
                    GameObject fmodel = fpath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(fpath) : null;
                    if (fmodel == null) { log.AppendLine("⚠ 모델 없음: " + fr.model); continue; }
                    Material m = MatFrom(root, fr.prefab + "_Body", body, fr.body, bodyFallback);
                    var accs = new Material[fr.accents.Length];
                    for (int k = 0; k < accs.Length; k++)
                        accs[k] = MatFrom(root, fr.prefab + "_Accent" + (k == 0 ? "" : (k + 1).ToString()), body, fr.accents[k], bodyFallback);
                    GameObject prefab = SavePrefab(root, fr.prefab, fmodel, m, face, Hex(fr.body.top), Hex(fr.body.bottom), fr.face, controller, droplet, accs, fr.aura);
                    if (prefab != null) made[fr.prefab] = prefab;
                }

                try { BuildStack(root, made); }
                catch (Exception e) { log.AppendLine("⚠ Slime_Stack 은 만들지 못했어요: " + e.Message); }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var folder = AssetDatabase.LoadAssetAtPath<Object>(root + "/Prefabs");
                if (folder != null) { Selection.activeObject = folder; EditorGUIUtility.PingObject(folder); }

                string msg = $"프리팹 {made.Count}개를 만들었어요 (Built-in 셰이더).\n위치: {root}/Prefabs\n\n{log}";
                Debug.Log("[Gravity Collect] " + msg);
                if (showDialog) EditorUtility.DisplayDialog("Gravity Collect Slime", msg, "확인");
                return made.Count > 0;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return Fail("만드는 중 오류가 났어요:\n" + e.GetType().Name + ": " + e.Message + "\n\n" + log, showDialog);
            }
        }

        static bool Fail(string msg, bool showDialog)
        {
            Debug.LogError("[Gravity Collect] " + msg);
            if (showDialog) EditorUtility.DisplayDialog("Gravity Collect Slime", msg, "확인");
            return false;
        }

        /// <summary>Finds an asset by search filter, preferring the exact file name.</summary>
        static string Find(string filter, string fileName)
        {
            string fallback = null;
            foreach (string g in AssetDatabase.FindAssets(filter))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (Path.GetFileName(p) == fileName) return p;
                fallback = fallback ?? p;
            }
            return fallback;
        }

        /// <summary>Shader by name, else by file name; null if missing or it failed to compile.</summary>
        static Shader PickShader(string shaderName, string fileName, StringBuilder log)
        {
            Shader s = Shader.Find(shaderName);
            if (s == null)
            {
                string p = Find(fileName + " t:Shader", fileName + ".shader");
                if (p != null) s = AssetDatabase.LoadAssetAtPath<Shader>(p);
            }
            if (s == null) { log.AppendLine("⚠ 셰이더 없음: " + shaderName); return null; }
            if (ShaderUtil.ShaderHasError(s)) { log.AppendLine("⚠ 셰이더 컴파일 오류: " + shaderName + " (Console에서 오류 내용 확인)"); return null; }
            return s;
        }

        static GameObject SavePrefab(string root, string prefabName, GameObject model, Material body, Material face, Color top, Color bottom, SlimeFace f,
                                     AnimatorController controller, Material droplet, Material[] accents = null, SlimeFriendsData.AuraDef aura = default)
        {
            var go = new GameObject(prefabName);
            try
            {
                GameObject visual = Object.Instantiate(model, go.transform, false);
                visual.name = "Visual";

                foreach (Renderer r in visual.GetComponentsInChildren<Renderer>())
                {
                    Material[] mats = r.sharedMaterials;
                    var mf = r.GetComponent<MeshFilter>();
                    int subs = mf != null && mf.sharedMesh != null ? mf.sharedMesh.subMeshCount : mats.Length;
                    if (mats.Length < subs) System.Array.Resize(ref mats, subs);   // one slot per submesh (body, face, accents)
                    bool named = false;
                    foreach (var mm in mats) if (mm != null && mm.name.ToLowerInvariant().Contains("slime_")) named = true;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (named)   // models imported with their OBJ material names
                        {
                            string n = mats[i] != null ? mats[i].name.ToLowerInvariant() : "";
                            mats[i] = n.Contains("face") ? face : body;
                            if (accents != null)
                                for (int k = accents.Length - 1; k >= 0; k--)
                                    if (n.Contains("accent" + (k == 0 ? "" : (k + 1).ToString()))) { mats[i] = accents[k]; break; }
                        }
                        else         // imported without materials (no URP defaults): submesh order is Body, Face, Accent, Accent2, Accent3
                        {
                            mats[i] = i == 0 ? body : i == 1 ? face : (accents != null && i - 2 < accents.Length ? accents[i - 2] : body);
                        }
                    }
                    r.sharedMaterials = mats;
                    r.shadowCastingMode = ShadowCastingMode.On;
                }

                var slime = go.AddComponent<SlimeCharacter>();
                slime.topColor = top;
                slime.bottomColor = bottom;
                slime.face = f;
                slime.visual = visual.transform;

                var col = go.AddComponent<SphereCollider>();
                col.center = new Vector3(0f, 0.38f, 0f);
                col.radius = 0.45f;

                if (controller != null)
                {
                    var animator = go.AddComponent<Animator>();
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    var motion = go.AddComponent<SlimeMotion>();
                    motion.splash = CreateSplash(go.transform, droplet);
                    if (aura.on) motion.aura = CreateAura(go.transform, droplet, aura);
                }

                return PrefabUtility.SaveAsPrefabAsset(go, $"{root}/Prefabs/{prefabName}.prefab");
            }
            finally { Object.DestroyImmediate(go); }
        }

        // ---------- animations ----------

        static AnimatorController BuildAnimations(string root)
        {
            string dir = root + "/Animations";
            EnsureFolder(dir);
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var c in SlimeAnimData.Clips)
            {
                string path = $"{dir}/Slime_{c.name}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
                clip.ClearCurves();
                clip.frameRate = 60f;
                foreach (var (field, keys) in c.tracks)
                {
                    var frames = new Keyframe[keys.Length / 2];
                    for (int i = 0; i < frames.Length; i++) frames[i] = new Keyframe(keys[i * 2], keys[i * 2 + 1]);
                    var curve = new AnimationCurve(frames);
                    var mode = field == "face" ? AnimationUtility.TangentMode.Constant : AnimationUtility.TangentMode.ClampedAuto;
                    for (int i = 0; i < frames.Length; i++)
                    {
                        AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
                        AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
                    }
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SlimeMotion), field), curve);
                }
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = c.loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                var events = new List<AnimationEvent>();
                foreach (var (time, function) in c.events) events.Add(new AnimationEvent { time = time, functionName = function });
                AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                EditorUtility.SetDirty(clip);
                clips[c.name] = clip;
            }

            string ctrlPath = dir + "/Slime.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null) AssetDatabase.DeleteAsset(ctrlPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Dead", AnimatorControllerParameterType.Trigger);

            var sm = ctrl.layers[0].stateMachine;
            var idle = sm.AddState("Idle", new Vector3(300, 0, 0));
            var move = sm.AddState("Move", new Vector3(300, 120, 0));
            var jump = sm.AddState("Jump", new Vector3(560, 60, 0));
            var death = sm.AddState("Death", new Vector3(560, 200, 0));
            idle.motion = clips["Idle"]; move.motion = clips["Move"]; jump.motion = clips["Jump"]; death.motion = clips["Death"];
            sm.defaultState = idle;

            var t = idle.AddTransition(move); t.hasExitTime = false; t.duration = 0.1f; t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            t = move.AddTransition(idle); t.hasExitTime = false; t.duration = 0.15f; t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            t = jump.AddTransition(idle); t.hasExitTime = true; t.exitTime = 0.95f; t.duration = 0.08f;
            var a = sm.AddAnyStateTransition(jump); a.hasExitTime = false; a.duration = 0.05f; a.canTransitionToSelf = false; a.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            a = sm.AddAnyStateTransition(death); a.hasExitTime = false; a.duration = 0.05f; a.canTransitionToSelf = false; a.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
            EditorUtility.SetDirty(ctrl);
            return ctrl;
        }

        static ParticleSystem CreateSplash(Transform parent, Material droplet)
        {
            var fx = new GameObject("SplashFX");
            fx.transform.SetParent(parent, false);
            fx.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            var ps = fx.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.16f);
            main.gravityModifier = 1.4f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 40;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)14) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.3f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            var pr = fx.GetComponent<ParticleSystemRenderer>();
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            if (droplet != null) pr.sharedMaterial = droplet;
            return ps;
        }

        static ParticleSystem CreateAura(Transform parent, Material droplet, SlimeFriendsData.AuraDef a)
        {
            var fx = new GameObject("AuraFX");
            fx.transform.SetParent(parent, false);
            fx.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            var ps = fx.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Color c = Hex(a.color);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(a.life * 0.7f, a.life * 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(a.speed * 0.5f, a.speed * 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(a.size * 0.6f, a.size * 1.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(c, Color.Lerp(c, Color.white, 0.5f));
            main.gravityModifier = a.gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 60;
            var emission = ps.emission;
            emission.rateOverTime = a.rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.55f;
            shape.radiusThickness = 0.2f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);
            var pr = fx.GetComponent<ParticleSystemRenderer>();
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            if (droplet != null) pr.sharedMaterial = droplet;
            return ps;
        }

        static void BuildStack(string root, Dictionary<string, GameObject> made)
        {
            if (!made.TryGetValue("Pink", out var a) || !made.TryGetValue("Purple", out var b) || !made.TryGetValue("Yellow", out var c)) return;
            var go = new GameObject("Slime_Stack");
            try
            {
                var stack = go.AddComponent<SlimeStack>();
                float y = 0f;
                (GameObject prefab, float scale, SlimeFace face)[] parts = { (a, 2.2f, SlimeFace.Happy), (b, 1.0f, SlimeFace.Wink), (c, 0.6f, SlimeFace.Starry) };
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = Object.Instantiate(parts[i].prefab, go.transform);      // plain copy, not a nested prefab
                    part.name = parts[i].prefab.name;
                    part.transform.localScale = Vector3.one * parts[i].scale;
                    part.transform.localPosition = new Vector3(0f, y, 0f);
                    y += 0.78f * parts[i].scale * 0.9f;
                    var s = part.GetComponent<SlimeCharacter>();
                    s.face = parts[i].face;
                    if (i > 0) foreach (var col in part.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(col);
                    stack.slimes.Add(s);
                }
                PrefabUtility.SaveAsPrefabAsset(go, $"{root}/Prefabs/Slime_Stack.prefab");
            }
            finally { Object.DestroyImmediate(go); }
        }

        static void SetupFaceTexture(string path)
        {
            if (path == null) return;
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            if (ti.alphaIsTransparency && ti.wrapMode == TextureWrapMode.Clamp && ti.npotScale == TextureImporterNPOTScale.None) return;
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.mipmapEnabled = true;
            ti.filterMode = FilterMode.Trilinear;
            ti.anisoLevel = 4;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.SaveAndReimport();
        }

        static Material LoadOrCreate(string root, string matName, Shader shader)
        {
            string path = $"{root}/Materials/{matName}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            return m;
        }

        static Color Hex(string h) => ColorUtility.TryParseHtmlString(h, out Color c) ? c : Color.white;

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
