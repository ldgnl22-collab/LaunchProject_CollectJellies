using System.Collections.Generic;
using UnityEngine;

/// 메시 합치기 / 영역 계산 공용 도구 (모바일 드로우콜 절감용)
public static class PuffyMeshUtil
{
    public const string GroundName = "_PuffyGround";
    public const string EdgeName = "_PuffyEdge";
    public const string PropsName = "_PuffyProps";
    const int MaxVerts = 60000;   // 16비트 인덱스 한도 안쪽 (구형 기기 호환)

    public struct Item
    {
        public Mesh mesh;
        public Matrix4x4 matrix;      // 월드 행렬
        public bool hasUv3;
        public Vector2 uv3;           // 땅: (윗면 높이, 1)
    }

    /// 자동 생성된 오브젝트인지 (원본으로 취급하지 않음)
    public static bool IsGenerated(Transform t)
    {
        for (; t != null; t = t.parent)
            if (t.name.StartsWith("_Puffy")) return true;
        return false;
    }

    /// root 아래 원본 길 메시들 (자동 생성물 제외, 꺼진 렌더러도 포함)
    public static List<MeshFilter> Sources(Transform root)
    {
        var list = new List<MeshFilter>();
        if (!root) return list;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh || IsGenerated(mf.transform)) continue;
            if (!mf.gameObject.activeInHierarchy) continue;
            list.Add(mf);
        }
        return list;
    }

    /// 렌더러가 꺼져 있어도 정확한 월드 바운드
    public static Bounds WorldBounds(MeshFilter mf)
    {
        var b = mf.sharedMesh.bounds;
        var m = mf.transform.localToWorldMatrix;
        var wb = new Bounds(m.MultiplyPoint3x4(b.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
        {
            var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            wb.Encapsulate(m.MultiplyPoint3x4(c));
        }
        return wb;
    }

    /// 아이템들을 cellSize 격자로 나눠 합침 → 칸마다 메시 1개 (화면 밖 칸은 자동으로 안 그림)
    public static Transform BuildChunks(List<Item> items, float cellSize, Transform parent, Material mat, string name)
    {
        var container = new GameObject(name).transform;
        container.SetParent(parent, false);
        var toLocal = container.worldToLocalMatrix;

        var groups = new Dictionary<Vector2Int, List<Item>>();
        foreach (var it in items)
        {
            if (!it.mesh) continue;
            if (Application.isPlaying && !it.mesh.isReadable)
            {
                Debug.LogWarning($"[Puffy] '{it.mesh.name}' 메시가 Read/Write 꺼져 있어 합칠 수 없어요. 에디터에서 미리 굽거나 Read/Write를 켜주세요.");
                continue;
            }
            Vector3 p = it.matrix.GetColumn(3);
            var key = new Vector2Int(Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.z / cellSize));
            if (!groups.TryGetValue(key, out var g)) groups[key] = g = new List<Item>();
            g.Add(it);
        }

        int chunk = 0;
        foreach (var kv in groups)
        {
            var batch = new List<Item>();
            int verts = 0;
            foreach (var it in kv.Value)
            {
                int vc = it.mesh.vertexCount * Mathf.Max(1, it.mesh.subMeshCount);
                if (verts + vc > MaxVerts && batch.Count > 0)
                {
                    Flush(batch, toLocal, container, mat, $"{name}_{chunk++}");
                    batch.Clear(); verts = 0;
                }
                batch.Add(it); verts += vc;
            }
            if (batch.Count > 0) Flush(batch, toLocal, container, mat, $"{name}_{chunk++}");
        }
        return container;
    }

    static void Flush(List<Item> batch, Matrix4x4 toLocal, Transform container, Material mat, string name)
    {
        var ci = new List<CombineInstance>();
        var temps = new List<Mesh>();
        foreach (var it in batch)
        {
            var src = it.mesh;
            if (it.hasUv3)
            {
                // 블록마다 윗면 높이를 UV3 에 기록한 임시 사본
                src = Object.Instantiate(it.mesh);
                var uv3 = new Vector2[src.vertexCount];
                for (int k = 0; k < uv3.Length; k++) uv3[k] = it.uv3;
                src.uv3 = uv3;
                temps.Add(src);
            }
            for (int s = 0; s < src.subMeshCount; s++)
                ci.Add(new CombineInstance { mesh = src, transform = toLocal * it.matrix, subMeshIndex = s });
        }
        var mesh = new Mesh { name = name };
        mesh.CombineMeshes(ci.ToArray(), true, true, false);
        foreach (var t in temps)
        {
            if (Application.isPlaying) Object.Destroy(t); else Object.DestroyImmediate(t);
        }
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);

        var go = new GameObject(name);
        go.transform.SetParent(container, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    public static void DestroyChild(Transform parent, string name)
    {
        if (!parent) return;
        var t = parent.Find(name);
        while (t)
        {
            if (Application.isPlaying) { t.SetParent(null); Object.Destroy(t.gameObject); }
            else Object.DestroyImmediate(t.gameObject);
            t = parent.Find(name);
        }
    }
}
