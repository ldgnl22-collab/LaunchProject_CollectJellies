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
                _followers[i].Follow();
            }
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
                 float back = first.Distance - distance;
                 Vector3 backDirection = first.Rotation * Vector3.back;
                                
                 position = first.Position + backDirection * back;
                 rotation = first.Rotation;
                 return;
            }

            if (distance >= last.Distance)
            {
                position = last.Position;
                rotation = last.Rotation;
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
                float t = Mathf.InverseLerp(a.Distance, b.Distance, distance);
                position = Vector3.Lerp(a.Position, b.Position, t);
                rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
                return;
            }

            position = last.Position;
            rotation = last.Rotation;
        }

        
        
        
        private void Record()
        {
            TrailPoint last = _points[_points.Count - 1];
            float step = Vector3.Distance(_transform.position, last.Position);
            
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

        private void CacheComponent()
        {
            _transform = transform;
        }
}