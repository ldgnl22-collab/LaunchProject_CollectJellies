using System.Collections.Generic;
using UnityEngine;

/// 길 주변(옆, 아래, 멀리)에 3D 소품을 흩뿌려 시차(깊이감)를 만듦.
/// 모바일용: 오브젝트를 하나하나 만들지 않고 메시를 격자 단위로 합침
///  → 소품 50개도 드로우콜 몇 개, 화면 밖 칸은 자동 컬링
public class PuffyPropScatter : MonoBehaviour
{
    public int count = 40;
    public int seed = 1;

    [Header("배치 범위 (m)")]
    [Tooltip("길에서 이만큼은 비워 둠")] public float clearance = 6f;
    public float minDistance = 12f, maxDistance = 90f;
    [Tooltip("길 높이 기준 아래/위 범위")] public float minHeight = -45f, maxHeight = 10f;
    public Vector2 scaleRange = new Vector2(0.8f, 2.2f);
    [Tooltip("멀수록 크게")] public float scaleByDistance = 0.02f;
    [Tooltip("합치는 격자 크기 (m)")] public float chunkSize = 60f;

    struct Part { public Mesh mesh; public Matrix4x4 m; }
    System.Random rnd;
    float R(float a, float c) => a + (float)rnd.NextDouble() * (c - a);

    public Transform Build(Transform area, List<MeshFilter> sources, GameObject[] prefabs, Material mat)
    {
        PuffyMeshUtil.DestroyChild(transform, PuffyMeshUtil.PropsName);
        if (!area || sources.Count == 0 || prefabs == null || prefabs.Length == 0 || !mat) return null;

        // 프리팹별 메시 목록 (프리팹 루트 기준 행렬)
        var parts = new List<List<Part>>();
        foreach (var p in prefabs)
        {
            if (!p) continue;
            var list = new List<Part>();
            var inv = p.transform.worldToLocalMatrix;
            foreach (var mf in p.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh) list.Add(new Part { mesh = mf.sharedMesh, m = inv * mf.transform.localToWorldMatrix });
            if (list.Count > 0) parts.Add(list);
        }
        if (parts.Count == 0) return null;

        var all = PuffyMeshUtil.WorldBounds(sources[0]);
        var boxes = new List<Bounds>();
        foreach (var mf in sources)
        {
            var b = PuffyMeshUtil.WorldBounds(mf);
            all.Encapsulate(b);
            b.Expand(clearance * 2);
            boxes.Add(b);
        }

        rnd = new System.Random(seed);
        var items = new List<PuffyMeshUtil.Item>();
        int placed = 0, tries = 0;
        while (placed < count && tries < count * 40)
        {
            tries++;
            var anchor = new Vector3(R(all.min.x, all.max.x), all.max.y, R(all.min.z, all.max.z));
            float ang = R(0, Mathf.PI * 2);
            float dist = Mathf.Lerp(minDistance, maxDistance, Mathf.Pow(R(0, 1), 0.7f));
            var pos = anchor + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * dist;
            pos.y = all.max.y + R(minHeight, maxHeight) * Mathf.Lerp(0.4f, 1f, dist / maxDistance);
            bool blocked = false;
            foreach (var e in boxes)
                if (pos.x > e.min.x && pos.x < e.max.x && pos.z > e.min.z && pos.z < e.max.z && pos.y > e.min.y - 25f) { blocked = true; break; }
            if (blocked) continue;

            float s = R(scaleRange.x, scaleRange.y) * (1 + dist * scaleByDistance);
            var place = Matrix4x4.TRS(pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * s);
            foreach (var part in parts[rnd.Next(parts.Count)])
                items.Add(new PuffyMeshUtil.Item { mesh = part.mesh, matrix = place * part.m });
            placed++;
        }
        return PuffyMeshUtil.BuildChunks(items, chunkSize, transform, mat, PuffyMeshUtil.PropsName);
    }
}
