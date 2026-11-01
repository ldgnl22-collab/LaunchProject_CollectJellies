using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Rauald.Editor
{
    // Editor-only mapping for upright side quads, including sloped paths.
    // Barrel-roll faces without vertical thickness edges are deliberately skipped.
    public static class LavaTileSideUV
    {
        [MenuItem("Tools/Rauald/Lava Tile/Fix Selected Side UV")]
        private static void FixSelectedSideUV()
        {
            int changed = 0, skipped = 0;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Fix Lava Tile Side UV");
            foreach (var go in Selection.gameObjects)
            {
                var pb = go.GetComponent<ProBuilderMesh>();
                if (pb == null || pb.selectedFaceCount == 0) continue;
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer == null) continue;
                var materials = renderer.sharedMaterials;
                var sourceUV = pb.textures;
                if (sourceUV == null || sourceUV.Count != pb.vertexCount)
                {
                    Debug.LogWarning("UV 데이터가 없는 메시입니다. ProBuilder에서 UV를 먼저 생성해주세요.", pb);
                    skipped += pb.selectedFaceCount;
                    continue;
                }
                var uv = sourceUV.ToArray();
                var plans = new List<(Face face, int topA, int bottomA, int topB, int bottomB, float width)>();
                foreach (var face in pb.GetSelectedFaces())
                {
                    if (face.distinctIndexes.Count != 4 || face.edges.Count != 4 ||
                        face.submeshIndex >= materials.Length || materials[face.submeshIndex] == null ||
                        materials[face.submeshIndex].name != "LavaTile_Side")
                    { skipped++; continue; }

                    var vertical = face.edges.Where(e =>
                    {
                        var delta = go.transform.TransformVector(pb.positions[e.b] - pb.positions[e.a]);
                        return delta.sqrMagnitude > 0.000001f &&
                            Mathf.Abs(Vector3.Dot(delta.normalized, Vector3.up)) > 0.999f;
                    }).ToArray();
                    if (vertical.Length != 2 || vertical[0].Contains(vertical[1].a) || vertical[0].Contains(vertical[1].b))
                    { skipped++; continue; }
                    int a = vertical[0].a, b = vertical[0].b;
                    int c = vertical[1].a, d = vertical[1].b;
                    if (go.transform.TransformPoint(pb.positions[a]).y < go.transform.TransformPoint(pb.positions[b]).y)
                        (a, b) = (b, a);
                    if (go.transform.TransformPoint(pb.positions[c]).y < go.transform.TransformPoint(pb.positions[d]).y)
                        (c, d) = (d, c);
                    // One texture repeat per world unit along the top edge.
                    float width = go.transform.TransformVector(pb.positions[c] - pb.positions[a]).magnitude;
                    if (width < 0.0001f) { skipped++; continue; }
                    plans.Add((face, a, b, c, d, width));
                }
                if (plans.Count == 0) continue;
                Undo.RegisterCompleteObjectUndo(pb, "Fix Lava Tile Side UV");
                var mesh = go.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null) Undo.RegisterCompleteObjectUndo(mesh, "Fix Lava Tile Side UV");
                foreach (var plan in plans)
                {
                    plan.face.manualUV = true;
                    plan.face.textureGroup = -1;
                    uv[plan.topA] = new Vector2(0, 1);
                    uv[plan.bottomA] = new Vector2(0, 0);
                    uv[plan.topB] = new Vector2(plan.width, 1);
                    uv[plan.bottomB] = new Vector2(plan.width, 0);
                }
                pb.textures = uv;
                // ProBuilder's optimized render mesh can have fewer vertices than its
                // editable data. RefreshUV uploads the entire editable UV array, so
                // restore matching topology before uploading it (including on reruns).
                bool rebuild = mesh == null || mesh.vertexCount != pb.vertexCount;
                if (rebuild)
                    pb.ToMesh();
                pb.RefreshUV(plans.Select(plan => plan.face));
                if (rebuild)
                {
                    // ToMesh clears the old vertex channels when the count changes.
                    // Rebuild them without reprojecting any unselected face UVs.
                    pb.Refresh(RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents |
                        RefreshMask.Collisions | RefreshMask.Bounds);
                    mesh = go.GetComponent<MeshFilter>().sharedMesh;
                }
                EditorUtility.SetDirty(pb);
                if (mesh != null) EditorUtility.SetDirty(mesh);
                PrefabUtility.RecordPrefabInstancePropertyModifications(pb);
                if (go.scene.IsValid()) EditorSceneManager.MarkSceneDirty(go.scene);
                changed += plans.Count;
            }
            Undo.CollapseUndoOperations(undoGroup);
            UnityEditor.ProBuilder.ProBuilderEditor.Refresh();
            SceneView.RepaintAll();
            EditorUtility.DisplayDialog("Lava Tile UV",
                $"수정한 옆면: {changed} / 제외한 면: {skipped}\n\n" +
                "LavaTile_Side가 적용된 사각형 옆면을 Face 모드에서 선택한 뒤 실행하세요.\n" +
                "수직인 두께 모서리가 있는 평지·경사면만 지원합니다.\n" +
                "수정한 면은 Manual UV가 됩니다. Ctrl+Z로 되돌릴 수 있습니다.\n씬 저장은 직접 해주세요.", "확인");
        }
    }
}
