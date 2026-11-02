// Generated from anims.json - the same keyframes drive the preview images.
namespace GravityCollect.EditorTools
{
    static class SlimeAnimData
    {
        public struct Clip
        {
            public string name;
            public float length;
            public bool loop;
            public (string field, float[] keys)[] tracks;   // keys = t0, v0, t1, v1, ...
            public (float time, string function)[] events;
        }

        public static readonly Clip[] Clips =
        {
            new Clip { name = "Idle", length = 1.4f, loop = true,
                tracks = new (string, float[])[]
                {
                    ("scaleY", new[] { 0f, 1f, 0.35f, 1.05f, 0.7f, 0.93f, 1.05f, 1.02f, 1.4f, 1f }),
                    ("scaleXZ", new[] { 0f, 1f, 0.35f, 0.97f, 0.7f, 1.05f, 1.05f, 0.99f, 1.4f, 1f }),
                    ("hop", new[] { 0f, 0f, 1.4f, 0f }),
                    ("tip", new[] { 0f, 0f, 0.35f, 0.22f, 0.7f, 0f, 1.05f, 0.06f, 1.4f, 0f }),
                    ("lean", new[] { 0f, 0f, 0.35f, 0.06f, 0.7f, -0.04f, 1.4f, 0f }),
                    ("gray", new[] { 0f, 0f, 1.4f, 0f }),
                    ("flash", new[] { 0f, 0f, 1.4f, 0f }),
                    ("faceFade", new[] { 0f, 0f, 1.4f, 0f }),
                    ("face", new[] { 0f, -1f, 1.4f, -1f }),
                },
                events = new (float, string)[] {  } },
            new Clip { name = "Move", length = 0.8f, loop = true,
                tracks = new (string, float[])[]
                {
                    ("scaleY", new[] { 0f, 1f, 0.15f, 1.12f, 0.3f, 1.04f, 0.45f, 0.72f, 0.6f, 0.82f, 0.8f, 1f }),
                    ("scaleXZ", new[] { 0f, 1f, 0.15f, 0.92f, 0.3f, 0.98f, 0.45f, 1.24f, 0.6f, 1.14f, 0.8f, 1f }),
                    ("hop", new[] { 0f, 0f, 0.2f, 0.1f, 0.4f, 0f, 0.8f, 0f }),
                    ("tip", new[] { 0f, 0.05f, 0.15f, 0.5f, 0.3f, 0.32f, 0.45f, 0f, 0.6f, 0f, 0.8f, 0.05f }),
                    ("lean", new[] { 0f, 0f, 0.15f, 0.4f, 0.3f, 0.55f, 0.45f, 0.12f, 0.6f, -0.12f, 0.8f, 0f }),
                    ("gray", new[] { 0f, 0f, 0.8f, 0f }),
                    ("flash", new[] { 0f, 0f, 0.8f, 0f }),
                    ("faceFade", new[] { 0f, 0f, 0.8f, 0f }),
                    ("face", new[] { 0f, -1f, 0.8f, -1f }),
                },
                events = new (float, string)[] {  } },
            new Clip { name = "Jump", length = 1f, loop = false,
                tracks = new (string, float[])[]
                {
                    ("scaleY", new[] { 0f, 1f, 0.12f, 0.68f, 0.22f, 1.28f, 0.45f, 1.1f, 0.62f, 0.96f, 0.76f, 0.66f, 0.88f, 1.08f, 1f, 1f }),
                    ("scaleXZ", new[] { 0f, 1f, 0.12f, 1.24f, 0.22f, 0.8f, 0.45f, 0.9f, 0.62f, 1.02f, 0.76f, 1.26f, 0.88f, 0.95f, 1f, 1f }),
                    ("hop", new[] { 0f, 0f, 0.15f, 0f, 0.3f, 0.8f, 0.45f, 1.1f, 0.6f, 0.8f, 0.75f, 0f, 1f, 0f }),
                    ("tip", new[] { 0f, 0f, 0.12f, 0f, 0.22f, 0.65f, 0.42f, 0.25f, 0.6f, 0f, 1f, 0f }),
                    ("lean", new[] { 0f, 0f, 1f, 0f }),
                    ("gray", new[] { 0f, 0f, 1f, 0f }),
                    ("flash", new[] { 0f, 0f, 1f, 0f }),
                    ("faceFade", new[] { 0f, 0f, 1f, 0f }),
                    ("face", new[] { 0f, -1f, 0.14f, 1f, 0.76f, 2f, 0.92f, -1f }),
                },
                events = new (float, string)[] {  } },
            new Clip { name = "Death", length = 1.6f, loop = false,
                tracks = new (string, float[])[]
                {
                    ("scaleY", new[] { 0f, 1f, 0.15f, 1.18f, 0.3f, 0.6f, 0.45f, 0.24f, 0.7f, 0.1f, 1.6f, 0.08f }),
                    ("scaleXZ", new[] { 0f, 1f, 0.15f, 0.86f, 0.3f, 1.25f, 0.45f, 1.5f, 0.7f, 1.5f, 1.6f, 1.55f }),
                    ("hop", new[] { 0f, 0f, 1.6f, 0f }),
                    ("tip", new[] { 0f, 0f, 0.15f, 0.4f, 0.3f, 0f, 1.6f, 0f }),
                    ("lean", new[] { 0f, 0f, 1.6f, 0f }),
                    ("gray", new[] { 0f, 0f, 0.1f, 0.85f, 0.3f, 0.85f, 0.5f, 0f, 1.6f, 0f }),
                    ("flash", new[] { 0f, 0f, 0.26f, 0f, 0.3f, 0.8f, 0.38f, 0f, 1.6f, 0f }),
                    ("faceFade", new[] { 0f, 0f, 0.38f, 0f, 0.55f, 1f, 1.6f, 1f }),
                    ("face", new[] { 0f, 8f, 0.15f, 10f }),
                },
                events = new (float, string)[] { (0.32f, "Splash"), (1.6f, "DeathFinished") } },
        };
    }
}
