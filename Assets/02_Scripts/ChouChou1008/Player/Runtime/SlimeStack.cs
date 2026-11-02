using System.Collections.Generic;
using UnityEngine;

namespace GravityCollect
{
    /// <summary>
    /// Stacks slimes on top of each other, each one riding the top of the one below with a little lag,
    /// so the tower wobbles when the bottom slime hops or squashes.
    /// Put it on any object and list the slimes from bottom to top.
    /// </summary>
    public class SlimeStack : MonoBehaviour
    {
        public List<SlimeCharacter> slimes = new List<SlimeCharacter>();
        [Tooltip("Model height of the dome slime at scale 1.")]
        public float slimeHeight = 0.78f;
        [Tooltip("How far each slime sinks into the one below (fraction of its own height).")]
        [Range(0f, 0.4f)] public float sink = 0.1f;
        [Tooltip("Higher = stiffer tower.")]
        public float follow = 18f;

        void LateUpdate()   // after SlimeCharacter has moved its visuals
        {
            float a = 1f - Mathf.Exp(-follow * Time.deltaTime);
            for (int i = 1; i < slimes.Count; i++)
            {
                SlimeCharacter below = slimes[i - 1], me = slimes[i];
                if (below == null || me == null || below.visual == null) continue;
                Transform v = below.visual;
                float selfSink = slimeHeight * sink * me.transform.lossyScale.y / Mathf.Max(0.0001f, v.lossyScale.y);
                Vector3 top = v.TransformPoint(new Vector3(0f, slimeHeight - selfSink, 0f));
                me.transform.position = Application.isPlaying ? Vector3.Lerp(me.transform.position, top, a) : top;
                me.transform.rotation = Quaternion.Slerp(me.transform.rotation, v.rotation, a);
            }
        }
    }
}
