using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GravityCollect
{
    /// <summary>
    /// Example: move a slime with the keyboard and let the Animator play Idle / Move / Jump / Death.
    ///   WASD or arrow keys = move,  Space = jump,  K = die,  R = respawn
    /// Put it on a slime prefab instance (one built by "Build Slime Prefabs (with Animations)").
    /// Works with both the old Input Manager and the new Input System.
    /// </summary>
    [RequireComponent(typeof(SlimeMotion))]
    public class SlimePlayerController : MonoBehaviour
    {
        public float moveSpeed = 2.5f;
        public float turnSpeed = 12f;
        public float jumpVelocity = 6f;
        public float gravity = 20f;

        SlimeMotion _motion;
        float _groundY, _vy;
        bool _airborne;

        void Start()
        {
            _motion = GetComponent<SlimeMotion>();
            _motion.useClipHop = false;          // this script does the real jump height
            _groundY = transform.position.y;
        }

        void Update()
        {
            if (_motion.IsDead)
            {
                if (Pressed(KeyR)) _motion.Respawn();
                return;
            }

            // 1. Move on the ground plane and face the way we go (the slime's face is +Z).
            Vector2 input = ReadMove();
            Vector3 dir = new Vector3(input.x, 0f, input.y);
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);

            // 2. Tell the Animator how fast we move -> Idle or Move.
            _motion.SetSpeed(dir.magnitude);

            // 3. Jump: real height here, squash/stretch from the Jump clip.
            if (!_airborne && Pressed(KeySpace))
            {
                _airborne = true;
                _vy = jumpVelocity;
                _motion.Jump();
            }
            if (_airborne)
            {
                _vy -= gravity * Time.deltaTime;
                Vector3 p = transform.position;
                p.y += _vy * Time.deltaTime;
                if (p.y <= _groundY) { p.y = _groundY; _airborne = false; _vy = 0f; }
                transform.position = p;
            }

            // 4. Death.
            if (Pressed(KeyK)) _motion.Die();
        }

        // ---------- input (old or new system) ----------
        const int KeySpace = 0, KeyK = 1, KeyR = 2;

        static bool Pressed(int key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            return key == KeySpace ? kb.spaceKey.wasPressedThisFrame : key == KeyK ? kb.kKey.wasPressedThisFrame : kb.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key == KeySpace ? KeyCode.Space : key == KeyK ? KeyCode.K : KeyCode.R);
#else
            return false;
#endif
        }

        static Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;
            float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
            return Vector2.zero;
#endif
        }
    }
}
