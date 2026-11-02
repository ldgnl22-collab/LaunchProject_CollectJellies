using UnityEngine;

namespace GravityCollect
{
    /// <summary>
    /// Demo helper for trying the slime in Play mode.
    /// With the Animator (SlimeMotion): 1 = Idle, 2 = Move, Space = Jump, K = Death, R = respawn,
    /// or turn on autoPlay to loop Idle -> Move -> Jump -> Death -> respawn by itself.
    /// Without it: A = next procedural animation, F = next face.
    /// Keys use the old Input Manager; autoPlay works with any input setting.
    /// </summary>
    [RequireComponent(typeof(SlimeCharacter))]
    public class SlimeShowcase : MonoBehaviour
    {
        public bool autoPlay = true;
        public float interval = 2f;

        SlimeCharacter _s;
        SlimeMotion _m;
        float _timer;
        int _step, _anim, _face;

        void Awake() { _s = GetComponent<SlimeCharacter>(); _m = GetComponent<SlimeMotion>(); }

        void Update()
        {
            if (autoPlay)
            {
                _timer += Time.deltaTime;
                if (_timer >= interval) { _timer = 0f; Step(); }
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            if (_m != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) _m.SetSpeed(0f);
                if (Input.GetKeyDown(KeyCode.Alpha2)) _m.SetSpeed(1f);
                if (Input.GetKeyDown(KeyCode.Space)) _m.Jump();
                if (Input.GetKeyDown(KeyCode.K)) _m.Die();
                if (Input.GetKeyDown(KeyCode.R)) _m.Respawn();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.A)) NextAnim();
                if (Input.GetKeyDown(KeyCode.F)) NextFace();
            }
#endif
        }

        void Step()
        {
            if (_m == null) { NextAnim(); return; }
            switch (_step++ % 5)
            {
                case 0: _m.SetSpeed(0f); break;                  // Idle
                case 1: _m.SetSpeed(1f); break;                  // Move
                case 2: _m.SetSpeed(0f); _m.Jump(); break;       // Jump
                case 3: _m.Die(); break;                         // Death
                case 4: _m.Respawn(); break;
            }
        }

        void NextAnim()
        {
            _anim = (_anim + 1) % System.Enum.GetValues(typeof(SlimeAnim)).Length;
            _s.Play((SlimeAnim)_anim);
            if (!SlimeCharacter.IsLoop((SlimeAnim)_anim)) _s.loop = SlimeAnim.Idle;
        }

        void NextFace()
        {
            _face = (_face + 1) % System.Enum.GetValues(typeof(SlimeFace)).Length;
            _s.autoFace = false;
            _s.SetFace((SlimeFace)_face);
        }
    }
}
