using UnityEngine;

namespace GravityCollect
{
    /// <summary>Faces in the SlimeFaces atlas (4 columns x 5 rows, left to right, top to bottom).</summary>
    public enum SlimeFace { Smile, Happy, Laugh, Wink, Love, Starry, Shy, Surprised, Sad, Angry, Dizzy, Sleepy, Blink, Cry, Yummy, Smug, Cheer, Cheeky, Delight, Cat }

    /// <summary>Procedural animations. Idle, Hop, Happy, Sleep and Dizzy loop; the rest play once and return to the loop.</summary>
    public enum SlimeAnim { Idle, Hop, Jump, Land, Happy, Hurt, Sleep, Spin, Dizzy }

    /// <summary>
    /// The dome slime: gradient colours, swappable face and nine procedural animations.
    /// Everything moves the "Visual" child (pivot at the base), so colliders and game logic on the root stay still.
    /// </summary>
    [DisallowMultipleComponent]
    public class SlimeCharacter : MonoBehaviour
    {
        [Header("Look")]
        public Color topColor = new Color(1f, 0.82f, 0.89f);
        public Color bottomColor = new Color(1f, 0.36f, 0.62f);
        public SlimeFace face = SlimeFace.Smile;

        [Header("Animation")]
        public SlimeAnim loop = SlimeAnim.Idle;
        [Tooltip("Animations switch to a matching face while they play (Hurt -> Dizzy, Sleep -> Sleepy, ...).")]
        public bool autoFace = true;
        [Tooltip("Blinks now and then while the face has open eyes.")]
        public bool blink = true;
        [Range(0.25f, 3f)] public float speed = 1f;
        public float hopHeight = 0.35f;
        public float jumpHeight = 1.2f;

        [Tooltip("Child that gets squashed and moved. Defaults to the first child.")]
        public Transform visual;

        /// <summary>Face forced by an animation clip (SlimeMotion); -1 = none.</summary>
        [HideInInspector] public int faceOverride = -1;

        public SlimeAnim Current => _oneShot ? _shot : loop;
        public event System.Action<SlimeAnim> Finished;

        static readonly int TopId = Shader.PropertyToID("_TopColor");
        static readonly int BottomId = Shader.PropertyToID("_BottomColor");
        static readonly int FaceId = Shader.PropertyToID("_FaceIndex");
        static readonly int FlashId = Shader.PropertyToID("_Flash");

        Renderer _renderer;
        int _bodySlot = 0, _faceSlot = -1;
        MaterialPropertyBlock _block;
        Vector3 _baseScale = Vector3.one, _basePos;
        Quaternion _baseRot = Quaternion.identity;

        bool _oneShot;
        SlimeAnim _shot;
        float _t, _loopT, _phase, _blinkTimer = 3f, _blinkLeft;
        bool _airborne;
        float _vy, _landTimer;

        void Awake()
        {
            if (visual == null) visual = transform.childCount > 0 ? transform.GetChild(0) : transform;
            _baseScale = visual.localScale;
            _basePos = visual.localPosition;
            _baseRot = visual.localRotation;
            _phase = Random.value * 10f;
            Apply(0f);
        }

        void OnValidate() { if (!Application.isPlaying) Apply(0f); }

        // ---------- public API ----------

        public void SetColors(Color top, Color bottom) { topColor = top; bottomColor = bottom; Apply(0f); }

        /// <summary>One-colour version (lighter top made automatically). Handy for the jelly train.</summary>
        public void SetColor(Color c) => SetColors(Color.Lerp(c, Color.white, 0.6f), c);

        public void SetFace(SlimeFace f) { face = f; Apply(0f); }

        /// <summary>Plays a one-shot (Jump, Land, Hurt, Spin) or switches the loop (Idle, Hop, Happy, Sleep, Dizzy).</summary>
        public void Play(SlimeAnim a)
        {
            if (IsLoop(a)) { loop = a; _oneShot = false; _loopT = 0f; return; }
            _shot = a; _oneShot = true; _t = 0f;
        }

        /// <summary>For game-driven jumps: call every frame while moving with the real vertical speed.</summary>
        public void SetAirborne(bool airborne, float verticalSpeed)
        {
            if (_airborne && !airborne) _landTimer = 0.2f;
            _airborne = airborne; _vy = verticalSpeed;
        }

        public static bool IsLoop(SlimeAnim a) => a == SlimeAnim.Idle || a == SlimeAnim.Hop || a == SlimeAnim.Happy || a == SlimeAnim.Sleep || a == SlimeAnim.Dizzy;

        public static float Duration(SlimeAnim a)
        {
            switch (a)
            {
                case SlimeAnim.Jump: return 0.95f;
                case SlimeAnim.Land: return 0.35f;
                case SlimeAnim.Hurt: return 0.55f;
                case SlimeAnim.Spin: return 0.8f;
                default: return 0f;
            }
        }

        // ---------- animation ----------

        SlimeMotion _motion;
        bool _motionChecked;

        void Update()
        {
            if (visual == null) return;
            if (!_motionChecked) { _motion = GetComponent<SlimeMotion>(); _motionChecked = true; }
            if (_motion != null && _motion.isActiveAndEnabled)
            {
                // Animator clips own the shape; here we only keep the face, colours and blinking up to date.
                Apply(-1f, loop);
                return;
            }
            float dt = Time.deltaTime * speed;
            _loopT += dt;

            float sy = 1f, sxz = 1f, y = 0f, shake = 0f, flash = 0f;
            Vector3 euler = Vector3.zero;
            SlimeAnim anim = loop;

            if (_oneShot)
            {
                anim = _shot;
                _t += dt;
                float d = Duration(_shot), k = Mathf.Clamp01(_t / d);
                switch (_shot)
                {
                    case SlimeAnim.Jump:
                        if (k < 0.18f) { float a = Mathf.Sin(k / 0.18f * Mathf.PI); sy = 1f - 0.25f * a; sxz = 1f + 0.15f * a; }
                        else if (k < 0.82f) { float a = (k - 0.18f) / 0.64f; y = jumpHeight * 4f * a * (1f - a); float up = 1f - 2f * a; sy = 1f + 0.22f * up; sxz = 1f - 0.1f * up; }
                        else { float a = Mathf.Sin((k - 0.82f) / 0.18f * Mathf.PI); sy = 1f - 0.28f * a; sxz = 1f + 0.18f * a; }
                        break;
                    case SlimeAnim.Land:
                        { float e = Mathf.Exp(-k * 4f) * Mathf.Cos(k * 18f); sy = 1f - 0.25f * e; sxz = 1f + 0.15f * e; }
                        break;
                    case SlimeAnim.Hurt:
                        { float e = 1f - k; shake = Mathf.Sin(_t * 70f) * 0.06f * e; sy = 1f - 0.18f * e; sxz = 1f + 0.12f * e; flash = (k < 0.35f && Mathf.Repeat(_t * 14f, 1f) < 0.5f) ? 0.7f : 0f; euler.z = Mathf.Sin(_t * 40f) * 8f * e; }
                        break;
                    case SlimeAnim.Spin:
                        { y = hopHeight * 1.6f * Mathf.Sin(k * Mathf.PI); euler.y = Ease(k) * 360f; float s = Mathf.Sin(k * Mathf.PI); sy = 1f + 0.12f * s; sxz = 1f - 0.06f * s; }
                        break;
                }
                if (_t >= d) { _oneShot = false; Finished?.Invoke(_shot); }
            }
            else
            {
                float t = _loopT + _phase;
                switch (loop)
                {
                    case SlimeAnim.Idle:
                        { float b = Mathf.Sin(t * 2.4f * Mathf.PI * 0.5f); sy = 1f + 0.04f * b; sxz = 1f - 0.025f * b; }
                        break;
                    case SlimeAnim.Hop:
                        { float p = Mathf.Repeat(t / 0.62f, 1f); float air = Mathf.Sin(p * Mathf.PI); y = hopHeight * air; float c = Mathf.Max(0f, 1f - air * 4f); sy = 1f + 0.12f * air - 0.18f * c; sxz = 1f - 0.06f * air + 0.12f * c; }
                        break;
                    case SlimeAnim.Happy:
                        { float p = Mathf.Repeat(t / 0.36f, 1f); float air = Mathf.Sin(p * Mathf.PI); y = hopHeight * 0.55f * air; sy = 1f + 0.1f * air - 0.1f * Mathf.Max(0f, 1f - air * 4f); sxz = 2f - sy; euler.z = Mathf.Sin(t * Mathf.PI / 0.72f) * 10f; }
                        break;
                    case SlimeAnim.Sleep:
                        { float b = Mathf.Sin(t * Mathf.PI * 0.9f); sy = 0.9f + 0.035f * b; sxz = 1.06f - 0.02f * b; }
                        break;
                    case SlimeAnim.Dizzy:
                        { euler.x = Mathf.Sin(t * 5f) * 9f; euler.z = Mathf.Cos(t * 5f) * 9f; sy = 1f + 0.03f * Mathf.Sin(t * 10f); sxz = 2f - sy; }
                        break;
                }
            }

            // game-driven jump stretch / landing squash on top of whatever is playing
            if (_airborne) { float k = Mathf.Clamp(_vy / 7.6f, -1f, 1f); sy *= 1f + 0.18f * k; sxz *= 1f - 0.08f * k; }
            else if (_landTimer > 0f) { _landTimer -= Time.deltaTime; float e = Mathf.Clamp01(_landTimer / 0.2f); sy *= 1f - 0.22f * e; sxz *= 1f + 0.12f * e; }

            visual.localScale = Vector3.Scale(_baseScale, new Vector3(sxz, sy, sxz));
            visual.localPosition = _basePos + new Vector3(shake, y, 0f);
            visual.localRotation = _baseRot * Quaternion.Euler(euler);

            Apply(flash, anim);
        }

        static float Ease(float k) => k < 0.5f ? 2f * k * k : 1f - Mathf.Pow(-2f * k + 2f, 2f) / 2f;

        SlimeFace FaceFor(SlimeAnim a)
        {
            if (!autoFace) return face;
            switch (a)
            {
                case SlimeAnim.Jump: return SlimeFace.Happy;
                case SlimeAnim.Land: return SlimeFace.Laugh;
                case SlimeAnim.Happy: return SlimeFace.Happy;
                case SlimeAnim.Hurt: return SlimeFace.Sad;
                case SlimeAnim.Sleep: return SlimeFace.Sleepy;
                case SlimeAnim.Spin: return SlimeFace.Starry;
                case SlimeAnim.Dizzy: return SlimeFace.Dizzy;
                default: return face;
            }
        }

        static bool OpenEyes(SlimeFace f) => f == SlimeFace.Smile || f == SlimeFace.Surprised;

        void Apply(float flash, SlimeAnim anim = SlimeAnim.Idle)
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
                if (_renderer == null) return;
                Material[] mats = _renderer.sharedMaterials;          // find which submesh is the face
                _faceSlot = -1;
                for (int i = 0; i < mats.Length; i++)
                    if (mats[i] != null && mats[i].HasProperty(FaceId)) _faceSlot = i;
                _bodySlot = _faceSlot == 0 && mats.Length > 1 ? 1 : 0;
            }
            if (_block == null) _block = new MaterialPropertyBlock();

            SlimeFace f = Application.isPlaying ? (faceOverride >= 0 ? (SlimeFace)faceOverride : FaceFor(anim)) : face;
            if (Application.isPlaying && blink && OpenEyes(f))
            {
                _blinkTimer -= Time.deltaTime;
                if (_blinkTimer <= 0f) { _blinkLeft = 0.12f; _blinkTimer = Random.Range(2.2f, 5f); }
                if (_blinkLeft > 0f) { _blinkLeft -= Time.deltaTime; f = SlimeFace.Blink; }
            }

            _renderer.GetPropertyBlock(_block, _bodySlot);
            _block.SetColor(TopId, topColor);
            _block.SetColor(BottomId, bottomColor);
            if (flash >= 0f) _block.SetFloat(FlashId, flash);
            _renderer.SetPropertyBlock(_block, _bodySlot);

            if (_faceSlot >= 0)
            {
                _renderer.GetPropertyBlock(_block, _faceSlot);
                _block.SetFloat(FaceId, (float)f);
                if (flash >= 0f) _block.SetFloat(FlashId, flash);
                _renderer.SetPropertyBlock(_block, _faceSlot);
            }
        }
    }
}
