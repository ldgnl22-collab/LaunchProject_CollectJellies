using UnityEditor;

namespace GravityCollect.EditorTools
{
    /// <summary>
    /// Runs "Build Slime Prefabs" once per editor session right after import, if the prefabs don't exist yet,
    /// so the coloured prefabs with faces are ready without touching the menu.
    /// </summary>
    [InitializeOnLoad]
    static class SlimeAutoSetup
    {
        const string Flag = "GravityCollect.SlimeAutoSetup.Tried";

        static SlimeAutoSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Flag, false)) return;
                if (AssetDatabase.FindAssets("DomeSlime t:Model").Length == 0) return;
                if (AssetDatabase.FindAssets("Jelly_Fire t:Prefab").Length > 0 && AssetDatabase.FindAssets("Slime_Pink t:Prefab").Length > 0) return;   // already built
                SessionState.SetBool(Flag, true);
                SlimePrefabBuilder.Build(true);
            };
        }
    }
}
