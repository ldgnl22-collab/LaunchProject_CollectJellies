// Generated from jellies.json (same values as the preview images).
using UnityEngine;

namespace GravityCollect.EditorTools
{
    static class SlimeFriendsData
    {
        public struct MatDef { public string top, bottom; public float g0, g1, glow, edge; public string pattern, patColor; public float patScale; public Vector2 scroll; }
        public struct AuraDef { public bool on; public string color; public float rate, speed, gravity, size, life; }
        public struct JellyDef { public string model, prefab; public SlimeFace face; public MatDef body; public MatDef[] accents; public AuraDef aura; }

        public static readonly JellyDef[] Jellies =
        {
            new JellyDef { model = "CrownJelly", prefab = "Jelly_Crown_Pink", face = SlimeFace.Smile,
                body = new MatDef { top = "#FFD6E8", bottom = "#FF6FA8", g0 = 0.02f, g1 = 0.82f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#FFF4B8", bottom = "#F0B42C", g0 = 0.77f, g1 = 0.93f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = false } },
            new JellyDef { model = "UnicornJelly", prefab = "Jelly_Unicorn_White", face = SlimeFace.Happy,
                body = new MatDef { top = "#F7F5FF", bottom = "#C9C1EC", g0 = 0.02f, g1 = 0.77f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#FFF2B0", bottom = "#E8A92C", g0 = 0.66f, g1 = 0.98f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#FF9FD0", bottom = "#A98CFF", g0 = 0.3f, g1 = 0.85f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#FFE3F6", rate = 4f, speed = 0.15f, gravity = -0.02f, size = 0.05f, life = 2.2f } },
            new JellyDef { model = "KnightJelly", prefab = "Jelly_Knight_Pink", face = SlimeFace.Delight,
                body = new MatDef { top = "#FFD6E8", bottom = "#FF6FA8", g0 = 0.02f, g1 = 0.82f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#D7DAE3", bottom = "#5D6170", g0 = 0.55f, g1 = 0.92f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#FF7A6B", bottom = "#B8232A", g0 = 0.85f, g1 = 1.35f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#2A2A33", bottom = "#121218", g0 = 0.5f, g1 = 0.8f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = false } },
            new JellyDef { model = "GalaxyJelly", prefab = "Jelly_Galaxy", face = SlimeFace.Surprised,
                body = new MatDef { top = "#7C5CFF", bottom = "#1A0F45", g0 = 0.02f, g1 = 0.74f, glow = 0f, edge = 0.85f, pattern = "Galaxy", patColor = "#FFFFFF", patScale = 1.2f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#FFD58A", bottom = "#E0843C", g0 = 0.9f, g1 = 1.14f, glow = 0.15f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#FFE9C8", bottom = "#F0A868", g0 = 0.95f, g1 = 1.1f, glow = 0.2f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#DCCBFF", rate = 7f, speed = 0.12f, gravity = 0f, size = 0.04f, life = 2.5f } },
            new JellyDef { model = "WizardJelly", prefab = "Jelly_Wizard", face = SlimeFace.Cheeky,
                body = new MatDef { top = "#A47CFF", bottom = "#3C1F8A", g0 = 0.02f, g1 = 0.8f, glow = 0f, edge = 0.7f, pattern = "Sparkle", patColor = "#FFFFFF", patScale = 1.3f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#5563E0", bottom = "#1F2463", g0 = 0.55f, g1 = 1.15f, glow = 0f, edge = 0.55f, pattern = "HatStars", patColor = "#FFFFFF", patScale = 2.2f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#4A3A66", bottom = "#21173A", g0 = 0.3f, g1 = 0.6f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#FFD36B", rate = 6f, speed = 0.25f, gravity = -0.05f, size = 0.045f, life = 1.8f } },
            new JellyDef { model = "CyberJelly", prefab = "Jelly_Cyber", face = SlimeFace.Wink,
                body = new MatDef { top = "#3FE6FF", bottom = "#FF3FCF", g0 = 0.02f, g1 = 0.74f, glow = 0.25f, edge = 0.6f, pattern = "Circuit", patColor = "#E6FEFF", patScale = 1.4f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#8A91A3", bottom = "#30343F", g0 = 0.3f, g1 = 0.9f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#C8FF7A", bottom = "#3AE05A", g0 = 0.3f, g1 = 1f, glow = 0.9f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) }, new MatDef { top = "#FF6FB8", bottom = "#3A2A4A", g0 = 0.6f, g1 = 0.9f, glow = 0f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#7FF6FF", rate = 9f, speed = 0.45f, gravity = 0f, size = 0.035f, life = 0.9f } },
            new JellyDef { model = "WaterJelly", prefab = "Jelly_Water", face = SlimeFace.Cheer,
                body = new MatDef { top = "#A8E6FF", bottom = "#2F7CFF", g0 = 0.02f, g1 = 0.75f, glow = 0.25f, edge = 0.9f, pattern = "Water", patColor = "#FFFFFF", patScale = 1.6f, scroll = new Vector2(0.03f, 0.02f) },
                accents = new[] { new MatDef { top = "#FFFFFF", bottom = "#8FD3FF", g0 = 0f, g1 = 1f, glow = 0.2f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#BFE8FF", rate = 5f, speed = 0.2f, gravity = -0.12f, size = 0.06f, life = 2f } },
            new JellyDef { model = "LeafJelly", prefab = "Jelly_Leaf", face = SlimeFace.Cheer,
                body = new MatDef { top = "#EEFF8E", bottom = "#4CBF3A", g0 = 0.02f, g1 = 0.6f, glow = 0.2f, edge = 0.8f, pattern = "Leaf", patColor = "#E2FFA0", patScale = 1.8f, scroll = new Vector2(0f, 0f) },
                accents = new[] { new MatDef { top = "#E4FF8E", bottom = "#3FAE3A", g0 = 0.6f, g1 = 0.88f, glow = 0.15f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#A6EF5A", rate = 3f, speed = 0.25f, gravity = 0.03f, size = 0.06f, life = 2.5f } },
            new JellyDef { model = "FireJelly", prefab = "Jelly_Fire", face = SlimeFace.Cheer,
                body = new MatDef { top = "#FF3418", bottom = "#FF9A2E", g0 = 0.02f, g1 = 0.5f, glow = 0.4f, edge = 1f, pattern = "Flame", patColor = "#FFD36A", patScale = 1.6f, scroll = new Vector2(0f, -0.35f) },
                accents = new[] { new MatDef { top = "#FFF1A0", bottom = "#FF7A1F", g0 = 0.5f, g1 = 1.15f, glow = 0.9f, edge = 0.55f, pattern = null, patColor = "#FFFFFF", patScale = 1.5f, scroll = new Vector2(0f, 0f) } },
                aura = new AuraDef { on = true, color = "#FFB347", rate = 10f, speed = 0.5f, gravity = -0.35f, size = 0.045f, life = 1.1f } },
        };
    }
}
