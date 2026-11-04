using UnityEngine;

namespace GravityCollect
{
    /// <summary>
    /// Shape channels for the slime animation clips (Idle, Move, Jump, Death).
    /// The AnimationClips animate the public fields below; this component turns them into
    /// scale / hop on the "Visual" child and the shader's drop-tip, lean, gray and flash.
    ///
    /// Drive it with the Animator parameters:  Speed (float), Jump (trigger), Dead (trigger).
    /// Or call the helpers: SetSpeed(), Jump(), Die().
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SlimeCharacter))]
    public class SlimeMotion : MonoBehaviour
    {
        [Header("Animated by clips")]
        public float scaleY = 1f;
        public float scaleXZ = 1f;
        [Tooltip("Visual lift in model units (1 = one slime height-ish).")]
        public float hop;
        [Tooltip("Pulls the top into a water-drop point.")]
        public float tip;
        [Tooltip("Leans the top forward (+) or back (-).")]
        public float lean;
        [Range(0f, 1f)] public float gray;
        [Range(0f, 1f)] public float flash;
        [Range(0f, 1f)] public float faceFade;
        [Tooltip("Face index while the clip plays; -1 keeps the slime's own face.")]
        public float face = -1f;

        [Header("Setup")]
        [Tooltip("Use the clip's own hop height. Turn off when game physics already moves the slime up.")]
        public bool useClipHop = true;
        [Tooltip("점프 높이 배율 (useClipHop이 켜져 있을 때만 적용)")]   // ★
        [Range(0.5f, 3f)] public float jumpHeight = 1f;                  // ★
        public ParticleSystem splash;
        [Tooltip("Looping ambient effect (embers, bubbles, sparkles...). Stops on death.")]
        public ParticleSystem aura;
        [Tooltip("Destroy the GameObject this many seconds after the death animation ends (0 = keep the puddle).")]
        public float destroyAfterDeath = 0f;

        public event System.Action Splashed;
        public event System.Action Died;

        public bool IsDead { get; private set; }

        static readonly int AnimSpeed = Animator.StringToHash("Speed");
        static readonly int AnimJump = Animator.StringToHash("Jump");
        static readonly int AnimDead = Animator.StringToHash("Dead");
        static readonly int TipId = Shader.PropertyToID("_Tip");
        static readonly int LeanId = Shader.PropertyToID("_Lean");
        static readonly int GrayId = Shader.PropertyToID("_Gray");
        static readonly int FlashId = Shader.PropertyToID("_Flash");
        static readonly int FadeId = Shader.PropertyToID("_FaceFade");

        SlimeCharacter _slime;
        Animator _animator;
        Renderer _renderer;
        MaterialPropertyBlock _block;
        Vector3 _baseScale = Vector3.one, _basePos;

        void Awake()
        {
            _slime = GetComponent<SlimeCharacter>();
            _animator = GetComponent<Animator>();
            if (_slime.visual == null) _slime.visual = transform.childCount > 0 ? transform.GetChild(0) : transform;
            _baseScale = _slime.visual.localScale;
            _basePos = _slime.visual.localPosition;
            _renderer = _slime.visual.GetComponentInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        // ---------- helpers for game code ----------
        public void SetSpeed(float speed) { if (_animator != null) _animator.SetFloat(AnimSpeed, speed); }
        public void Jump() { if (_animator != null && !IsDead) _animator.SetTrigger(AnimJump); }
        public void Die()
        {
            if (IsDead) return;
            IsDead = true;
            if (_animator != null) _animator.SetTrigger(AnimDead);
            if (aura != null) aura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        /// <summary>Brings a dead slime back (resets the Animator to Idle).</summary>
        public void Respawn()
        {
            IsDead = false;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;
            if (_animator != null) { _animator.Rebind(); _animator.Update(0f); }
            if (aura != null) aura.Play(true);
        }

        // ---------- animation events (called by the clips) ----------
        public void Splash()
        {
            if (splash != null)
            {
                var main = splash.main;
                main.startColor = new ParticleSystem.MinMaxGradient(_slime.bottomColor, Color.Lerp(_slime.bottomColor, _slime.topColor, 0.6f));
                splash.Play(true);
            }
            Splashed?.Invoke();
        }

        public void DeathFinished()
        {
            Died?.Invoke();
            if (destroyAfterDeath > 0f) Destroy(gameObject, destroyAfterDeath);
        }

        // ---------- apply ----------
        void LateUpdate()   // after the Animator has written the fields
        {
            Transform v = _slime.visual;
            if (v == null) return;
            v.localScale = Vector3.Scale(_baseScale, new Vector3(scaleXZ, scaleY, scaleXZ));
            v.localPosition = _basePos + Vector3.up * (useClipHop ? hop * _baseScale.y * jumpHeight : 0f);   // ★
            _slime.faceOverride = face >= 0f ? Mathf.RoundToInt(face) : -1;

            if (_renderer == null) return;
            int n = _renderer.sharedMaterials.Length;
            for (int i = 0; i < n; i++)
            {
                _renderer.GetPropertyBlock(_block, i);
                _block.SetFloat(TipId, tip);
                _block.SetFloat(LeanId, lean);
                _block.SetFloat(GrayId, gray);
                _block.SetFloat(FlashId, flash);
                _block.SetFloat(FadeId, faceFade);
                _renderer.SetPropertyBlock(_block, i);
            }
        }
    }
}