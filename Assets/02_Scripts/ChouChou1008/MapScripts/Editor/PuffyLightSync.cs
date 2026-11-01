using UnityEngine;

/// Directional Light 방향/색을 Puffy 셰이더에 전달 (Built-in / URP 공용).
/// 값이 바뀔 때만 전송해서 매 프레임 비용 거의 0.
[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class PuffyLightSync : MonoBehaviour
{
    static readonly int DirId = Shader.PropertyToID("_PuffyLightDir");
    static readonly int ColId = Shader.PropertyToID("_PuffyLightColor");
    Light lt;
    Vector3 lastDir;
    Color lastCol;
    float lastIntensity = -1;

    void OnEnable() { lt = GetComponent<Light>(); Push(true); }

    void LateUpdate() => Push(false);

    void OnDisable()
    {
        Shader.SetGlobalVector(DirId, Vector4.zero);
        Shader.SetGlobalVector(ColId, Vector4.zero);
    }

    void Push(bool force)
    {
        if (!lt) return;
        Vector3 toLight = -transform.forward;
        if (!force && toLight == lastDir && lt.color == lastCol && lt.intensity == lastIntensity) return;
        lastDir = toLight; lastCol = lt.color; lastIntensity = lt.intensity;
        Shader.SetGlobalVector(DirId, new Vector4(toLight.x, toLight.y, toLight.z, 0));
        Color c = lt.color * Mathf.Clamp(lt.intensity, 0f, 1.5f);
        Shader.SetGlobalVector(ColId, new Vector4(c.r, c.g, c.b, 1));
    }
}
