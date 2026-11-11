using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityCollect.EditorTools
{
    /// <summary>
    /// Tools > Gravity Collect > Convert Slime Materials to URP
    /// Switches every slime / jelly material from "Slime Body (Built-in)" to "Slime Body (URP)".
    /// Both shaders use the same property names, so colours, gradients, patterns and glow are kept.
    /// Also finds materials whose shader is already broken (pink) but still hold the slime colours.
    /// </summary>
    public static class SlimeURPConverter
    {
        const string BuiltinBody = "GravityCollect/Slime Body (Built-in)";
        const string UrpBody = "GravityCollect/Slime Body (URP)";
        const string ErrorShader = "Hidden/InternalErrorShader";

        [MenuItem("Tools/Gravity Collect/Convert Slime Materials to URP")]
        public static void Convert()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Dialog("지금 프로젝트는 URP가 아니에요 (Built-in).\n먼저 Render Pipeline Converter로 URP로 바꾼 뒤 실행해 주세요.");
                return;
            }

            Shader urp = Shader.Find(UrpBody);
            if (urp == null)
            {
                Dialog("SlimeURP.shader 를 찾을 수 없어요.\n슬라임 Shaders 폴더에 SlimeURP.shader 를 넣었는지 확인해 주세요.");
                return;
            }
            if (ShaderUtil.ShaderHasError(urp))
            {
                Dialog("SlimeURP.shader 에 컴파일 오류가 있어요.\nConsole 창의 빨간 오류를 확인해 주세요.");
                return;
            }

            var converted = new List<string>();
            var check = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null) continue;

                string shaderName = m.shader != null ? m.shader.name : ErrorShader;

                bool isSlimeBody = shaderName == BuiltinBody
                                   || (shaderName == ErrorShader && HasSavedColor(m, "_TopColor"));
                if (isSlimeBody)
                {
                    Undo.RecordObject(m, "Convert Slime Material to URP");
                    m.shader = urp;            // same property names -> values are kept
                    m.enableInstancing = true;
                    EditorUtility.SetDirty(m);
                    converted.Add(path);
                    continue;
                }

                // other materials in slime / jelly folders that are still not URP (particles etc.)
                string lower = path.ToLowerInvariant();
                bool slimeFolder = lower.Contains("slime") || lower.Contains("jelly") || lower.Contains("gravitycollect");
                bool urpReady = shaderName.StartsWith("Universal Render Pipeline/")
                                || shaderName.StartsWith("GravityCollect/")
                                || shaderName.StartsWith("Sprites/")
                                || shaderName.StartsWith("UI/")
                                || shaderName.StartsWith("Shader Graphs/");
                if (slimeFolder && !urpReady) check.Add($"{path}  ({shaderName})");
            }

            AssetDatabase.SaveAssets();

            var msg = new StringBuilder();
            msg.AppendLine($"슬라임 머티리얼 {converted.Count}개를 URP 셰이더로 바꿨어요.");
            if (check.Count > 0)
            {
                msg.AppendLine();
                msg.AppendLine($"⚠ 확인이 필요한 머티리얼 {check.Count}개 (Console에 목록):");
                msg.AppendLine("파티클이면 Shader를 Universal Render Pipeline/Particles/Unlit 으로 바꿔 주세요.");
            }

            Debug.Log("[Gravity Collect] URP 변환 완료\n" + string.Join("\n", converted));
            if (check.Count > 0) Debug.LogWarning("[Gravity Collect] 확인 필요\n" + string.Join("\n", check));
            Dialog(msg.ToString());
        }

        /// <summary>True if the material file still stores this colour, even when its shader is missing.</summary>
        static bool HasSavedColor(Material m, string property)
        {
            var so = new SerializedObject(m);
            SerializedProperty colors = so.FindProperty("m_SavedProperties.m_Colors");
            if (colors == null) return false;
            for (int i = 0; i < colors.arraySize; i++)
            {
                SerializedProperty first = colors.GetArrayElementAtIndex(i).FindPropertyRelative("first");
                if (first != null && first.stringValue == property) return true;
            }
            return false;
        }

        static void Dialog(string msg)
        {
            EditorUtility.DisplayDialog("Gravity Collect Slime (URP)", msg, "확인");
        }
    }
}
