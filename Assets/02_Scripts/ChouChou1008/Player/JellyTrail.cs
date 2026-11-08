using System.Collections.Generic;
using UnityEngine;

public class JellyTrail : MonoBehaviour
{
    [Header("Trail Settings")]
        [SerializeField] private float _spacing = 1.5f; //젤리 사이 간격
        [SerializeField] private float _recordStep = 0.05f; //Dealy

        private struct TrailPoint
        {
            public Vector3 Position; 
            public Quaternion Rotation;
            public float Distance;
        }

        private readonly List<TrailPoint> _points = new List<TrailPoint>();
        private readonly List<float> _jumpDistances = new List<float>();
        private readonly List<JellyFollower> _followers = new List<JellyFollower>();

        private Transform _transform;
        private float _totalDistance;

        public float TotalDistance => _totalDistance;
        public float Spacing => _spacing;
        public int FollowerCount => _followers.Count;
        
        
        
        private void Awake() => CacheComponent();

        private void Start() => AddPoint();

        private void LateUpdate()
        {
            Record();
            
            for (int i = 0; i < _followers.Count; i++)
            {
                _followers[i].Follow(); //i번째 젤리 위치와 방향.
            }
        }
        public void ResetTrail()  //초기화
        {
            _followers.Clear();
            _points.Clear();
            _jumpDistances.Clear();
            _totalDistance = 0f;
            AddPoint();
        }
        
        
        
        public void AddFollower(JellyFollower follower)
        {
            follower.Init(this, _followers.Count);
            _followers.Add(follower);
        }

        public void RecordJump()
        {
            _jumpDistances.Add(_totalDistance);
        }

        public bool HasJump(float from, float to)
        {
            for (int i = 0; i < _jumpDistances.Count; i++)
            {
                if (_jumpDistances[i] > from && _jumpDistances[i] <= to)
                {
                    return true;
                }
            }
            return false;
        }

        public void Sample(float distance, out Vector3 position, out Quaternion rotation)
        {
            TrailPoint first = _points[0];
            TrailPoint last = _points[_points.Count - 1];

            if (distance <= first.Distance)
            {
                 /*float back = first.Distance - distance;
                 Vector3 backDirection = first.Rotation * Vector3.back;
                                
                 position = first.Position + backDirection * back;
                 rotation = first.Rotation;*/
                 
                 position = first.Position;
                 rotation = first.Rotation;
                 return;
            }
            
            for (int i = _points.Count - 1; i > 0; i--)
            {
                TrailPoint a = _points[i - 1];
                if (distance < a.Distance)
                {
                    continue;
                }

                TrailPoint b = _points[i];
                // 매 프레임 마다 젤리가 서야 할 위치와 방향 계산
                float t = Mathf.InverseLerp(a.Distance, b.Distance, distance);
                // a -> b까지 몇% 지점  현 프레임에서 젤리 a에 가까운지 b에 가까운지
                position = Vector3.Lerp(a.Position, b.Position, t);
                // 현 프레임의 위치
                rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
                // 현 프레임의 방향 -> 필요한 이유 지금 젤리가 회전을 Quaternion으로 계산하고 있어서 그럼
                return;
            }

            position = last.Position;
            rotation = last.Rotation;
        }

        
        
        
         private void Record()
         {
             TrailPoint last = _points[_points.Count - 1];
         
             Vector3 move = _transform.position - last.Position;
             float step = Vector3.ProjectOnPlane(move, _transform.up).magnitude;
             // 현재 면의 위아래 이동(점프)은 빼고, 면을 따라 이동한 거리만 계산
         
             if (step < _recordStep)
             {
                 return;
             }
         
             _totalDistance += step;
             AddPoint();
             RemoveOldData();
         }

        private void AddPoint()
        {
            TrailPoint point = new TrailPoint();
            point.Position = _transform.position;
            point.Rotation = _transform.rotation;
            point.Distance = _totalDistance;
            _points.Add(point);
        }

        private void RemoveOldData()
        {
            float needDistance = _totalDistance - _spacing * (_followers.Count + 2);

            while (_points.Count > 2 && _points[1].Distance < needDistance)
            {
                _points.RemoveAt(0);
            }

            while (_jumpDistances.Count > 0 && _jumpDistances[0] < needDistance)
            {
                _jumpDistances.RemoveAt(0);
            }
        }
        public JellyFollower RemoveLastFollower()  //젤리 소멸
        {
            if (_followers.Count == 0)  //사망
            {
                return null;
            }

            int lastIndex = _followers.Count - 1;
            JellyFollower follower = _followers[lastIndex];
            _followers.RemoveAt(lastIndex);
            return follower;
        }

        private void CacheComponent()
        {
            _transform = transform;
        }
}