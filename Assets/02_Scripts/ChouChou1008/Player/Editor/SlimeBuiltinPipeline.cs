using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityCollect.EditorTools
{
    /// <summary>
    /// Keeps the slime assets on the Built-in render pipeline (no URP).
    /// 1) The slime .obj models import with NO materials, so Unity never creates "default" URP/Lit materials for them.
    /// 2) Tools > Gravity Collect > Switch Project to Built-in : clears the render pipeline asset from Graphics and every
    ///    Quality level, then rebuilds the slime prefabs with the Built-in shaders.
    /// </summary>
    public class SlimeBuiltinPipeline : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.Replace('\\', '/').Contains("GravityCollect/Slime/Models/")) return;
            var mi = (ModelImporter)assetImporter;
            if (mi.materialImportMode != ModelImporterMaterialImportMode.None)
                mi.materialImportMode = ModelImporterMaterialImportMode.None;   // no default (URP/Lit) materials inside the models
        }

        [MenuItem("Tools/Gravity Collect/Switch Project to Built-in (remove URP)")]
        public static void SwitchToBuiltin()
        {
            bool urp = GraphicsSettings.currentRenderPipeline != null;
            if (urp && !EditorUtility.DisplayDialog("Gravity Collect",
                    "프로젝트 렌더 파이프라인을 URP에서 Built-in으로 바꿀까요?\n\n" +
                    "· Graphics / Quality 설정의 URP 에셋을 비워요 (URP 패키지와 에셋 파일은 지우지 않아요)\n" +
                    "· 슬라임 프리팹을 Built-in 셰이더로 다시 만들어요\n" +
                    "· 이 프로젝트의 다른 URP 머티리얼은 보라색으로 보일 수 있어요", "바꾸기", "취소"))
                return;

#if UNITY_2021_2_OR_NEWER
            GraphicsSettings.defaultRenderPipeline = null;
#else
            GraphicsSettings.renderPipelineAsset = null;
#endif
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = null;
            }
            QualitySettings.SetQualityLevel(current, false);
            AssetDatabase.SaveAssets();

            // re-import the models without materials, then rebuild prefabs with the Built-in shaders
            foreach (string g in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.Contains("GravityCollect/Slime/Models/")) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }
            SlimePrefabBuilder.Build(true);
            Debug.Log("[Gravity Collect] Render pipeline set to Built-in. Slime prefabs rebuilt without URP.");
        }
    }
}
