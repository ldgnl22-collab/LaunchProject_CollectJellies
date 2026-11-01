using System.Collections.Generic;
using UnityEngine;

/// 길(박스) 윗면 모서리를 따라 볼록한 3D 쿠션 테두리를 만듦.
/// 모바일용: 윗반구만 있는 저폴리 쿠션(삼각형 32개) + 격자 단위로 합쳐서 드로우콜 몇 개로 끝냄.
public class PuffyEdgeBuilder : MonoBehaviour
{
    [Tooltip("쿠션 하나 크기 (m) — 크게 할수록 개수가 줄어 가벼워짐")] public float bumpSize = 0.6f;
    [Tooltip("쿠션 간격 (크기 대비)")] public float spacing = 0.85f;
    [Range(0.3f, 1f)] public float flatten = 0.6f;
    public float overhang = 0.12f;
    [Tooltip("합치는 격자 크기 (m)")] public float chunkSize = 40f;

    struct Edge
    {
        public Vector3 a, dir, outv; public float len;
        public Edge(Vector3 a, Vector3 dir, float len, Vector3 outv) { this.a = a; this.dir = dir; this.len = len; this.outv = outv; }
    }

    public Transform Build(Transform root, List<MeshFilter> sources, Material mat, Color color)
    {
        PuffyMeshUtil.DestroyChild(root, PuffyMeshUtil.EdgeName);
        if (!root || sources.Count == 0 || !mat) return null;

        var boxes = new List<Bounds>();
        foreach (var mf in sources) boxes.Add(PuffyMeshUtil.WorldBounds(mf));
        var bump = MakeDome(8, 3, color);
        var items = new List<PuffyMeshUtil.Item>();
        var rnd = new System.Random(12345);
        float r = bumpSize * 0.5f;

        for (int bi = 0; bi < boxes.Count; bi++)
        {
            var b = boxes[bi];
            float top = b.max.y;
            var edges = new[]
            {
                new Edge(new Vector3(b.min.x, top, b.min.z), Vector3.right,   b.size.x, Vector3.back),
                new Edge(new Vector3(b.min.x, top, b.max.z), Vector3.right,   b.size.x, Vector3.forward),
                new Edge(new Vector3(b.min.x, top, b.min.z), Vector3.forward, b.size.z, Vector3.left),
                new Edge(new Vector3(b.max.x, top, b.min.z), Vector3.forward, b.size.z, Vector3.right),
            };
            foreach (var e in edges)
            {
                int n = Mathf.Max(1, Mathf.RoundToInt(e.len / (bumpSize * spacing)));
                for (int k = 0; k <= n; k++)
                {
                    Vector3 p = e.a + e.dir * (e.len * k / n);
                    Vector3 probe = p + e.outv * (r + 0.05f) + Vector3.down * 0.05f;
                    if (InsideOther(boxes, bi, probe)) continue;
                    Vector3 c = p + e.outv * overhang + Vector3.down * r * 0.55f;
                    float s = bumpSize * (0.92f + 0.16f * (float)rnd.NextDouble());
                    items.Add(new PuffyMeshUtil.Item
                    {
                        mesh = bump,
                        matrix = Matrix4x4.TRS(c, Quaternion.Euler(0, (float)rnd.NextDouble() * 360f, 0), new Vector3(s, s * flatten, s))
                    });
                }
            }
        }
        return PuffyMeshUtil.BuildChunks(items, chunkSize, root, mat, PuffyMeshUtil.EdgeName);
    }

    static bool InsideOther(List<Bounds> boxes, int self, Vector3 p)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (i == self) continue;
            var b = boxes[i]; b.Expand(0.02f);
            if (b.Contains(p)) return true;
        }
        return false;
    }

    /// 윗반구 + 살짝 아래로 내려온 띠 (아래쪽은 박스에 가려지므로 생략)
    static Mesh MakeDome(int lon, int lat, Color color)
    {
        var v = new List<Vector3>(); var nrm = new List<Vector3>(); var tri = new List<int>(); var col = new List<Color>();
        for (int y = 0; y <= lat + 1; y++)
        {
            float a = y <= lat ? (Mathf.PI * 0.5f) * y / lat : Mathf.PI * 0.62f;   // 마지막 줄은 적도 아래로 조금
            for (int x = 0; x <= lon; x++)
            {
                float b = 2 * Mathf.PI * x / lon;
                var p = new Vector3(Mathf.Sin(a) * Mathf.Cos(b), Mathf.Cos(a), Mathf.Sin(a) * Mathf.Sin(b));
                v.Add(p * 0.5f); nrm.Add(p); col.Add(color);
            }
        }
        for (int y = 0; y <= lat; y++)
            for (int x = 0; x < lon; x++)
            {
                int i0 = y * (lon + 1) + x, i1 = i0 + lon + 1;
                tri.AddRange(new[] { i0, i0 + 1, i1, i1, i0 + 1, i1 + 1 });
            }
        var m = new Mesh { name = "PuffyDome" };
        m.SetVertices(v); m.SetNormals(nrm); m.SetColors(col); m.SetTriangles(tri, 0);
        return m;
    }
}
