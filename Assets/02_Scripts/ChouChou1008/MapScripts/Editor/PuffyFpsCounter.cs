using UnityEngine;

/// 기기 테스트용 FPS / 품질 표시 (출시 빌드에서는 오브젝트를 끄거나 지우세요)
public class PuffyFpsCounter : MonoBehaviour
{
    float acc; int frames; string text = "";
    GUIStyle style;

    void Update()
    {
        acc += Time.unscaledDeltaTime; frames++;
        if (acc >= 0.5f)
        {
            text = $"{frames / acc:0} FPS  {acc / frames * 1000f:0.0}ms  {PuffyMobileBoot.CurrentTier}";
            acc = 0; frames = 0;
        }
    }

    void OnGUI()
    {
        if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 40) };
        GUI.Label(new Rect(12, 12, 600, 80), text, style);
    }
}
